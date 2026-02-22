using GitGudModsListLoader.Models;
using Microsoft.Extensions.Options;
using NGitLab;
using NGitLab.Models;
using SharpConfig;
using System.Text.Json;

namespace GitGudModsListLoader.Services;

public class ModsListClient(
    IOptions<GitLabOptions> options,
    ILogger<ModsListClient> logger,
    IGitLabClient client) : IModsListClient
{
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<ModsListInfo> GetModsListAsync(CancellationToken cancellationToken)
    {
        ModsListInfo? modsList = default;
        async Task ParseModsList(Stream stream)
        {
            modsList = await JsonSerializer.DeserializeAsync<ModsListInfo>(
                stream,
                _serializerOptions,
                cancellationToken);

            logger.LogInformation("Found mods in modslist: {Count}", modsList?.Mods.Count ?? 0);
        }

        await client.GetRepository(options.Value.ModsList.ProjectId)
            .Files.GetRawAsync(
                "gitgud-modslist.json",
                ParseModsList,
                new() { Ref = options.Value.ModsList.Branch },
                cancellationToken);

        if (modsList is null)
        {
            throw new InvalidOperationException("Unable to deserialize mods list");
        }

        return modsList;
    }

    public Task<Project> GetProjectInfoAsync(ProjectId projectId, CancellationToken cancellationToken) =>
        client.Projects.GetAsync(projectId, new(), cancellationToken);

    public async Task<Dictionary<string, Dictionary<string, string>>> GetModMetadataAsync(ProjectId projectId, string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Argument is empty", nameof(path));
        }

        if (path[0] == '/')
        {
            path = path[1..];
        }

        var metadataFile = await client.GetRepository(projectId)
            .Files.GetAsync(path, "HEAD", cancellationToken);

        logger.LogInformation("Downloaded metadata file {MetadataPath} for project {ProjectId}", path, projectId);

        //var metadata = _iniParser.Parse(metadataFile.DecodedContent);
        Configuration? metadata = Configuration.LoadFromString(metadataFile.DecodedContent);

        return metadata.ToDictionary(
            section => section.Name,
            section => section.ToDictionary(
                keyData => keyData.Name,
                keyData => keyData.RawValue));
    }

    public IAsyncEnumerable<ReleaseInfo> GetProjectReleasesAsync(ProjectId projectId) =>
        client.GetReleases(projectId).GetAsync(new());

    public IAsyncEnumerable<Tag> GetProjectTagsAsync(ProjectId projectId) =>
        client.GetRepository(projectId).Tags.GetAsync(new());

    public Uri GetCommitArchiveUrl(ProjectId projectId, Sha1 commit) =>
        new(new Uri(options.Value.Host), $"api/v4/projects/{projectId}/repository/archive.zip?sha={commit}");

    public async Task<IEnumerable<ModInfo>> GetModsDbAsync(CancellationToken cancellationToken)
    {
        ModInfo[]? modsDb = default;
        async Task ParseModsDb(Stream stream)
        {
            modsDb = await JsonSerializer.DeserializeAsync<ModInfo[]>(
                stream,
                _serializerOptions,
                cancellationToken);
        }

        await client.GetRepository(options.Value.ModsList.ProjectId)
            .Files.GetRawAsync(
                "modslist-db.json",
                ParseModsDb,
                new() { Ref = options.Value.ModsList.Branch },
                cancellationToken);

        if (modsDb is null)
        {
            throw new InvalidOperationException("Unable to deserialize mods db");
        }

        return modsDb;
    }

    public Task UpdateModsDbAsync(IEnumerable<ModInfo> mods, CancellationToken cancellationToken)
    {
        string content = JsonSerializer.Serialize(mods, _serializerOptions);

        return client.GetRepository(options.Value.ModsList.ProjectId)
            .Files.UpdateAsync(
                new()
                {
                    Path = "modslist-db.json",
                    Branch = options.Value.ModsList.Branch,
                    CommitMessage = $"Updated mods list {DateTime.Now}",
                    RawContent = content
                },
                cancellationToken);
    }
}
