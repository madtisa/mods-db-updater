using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NGitLab;
using NGitLab.Models;
using SharpConfig;
using YamlDotNet.RepresentationModel;

namespace GitGudModsListLoader.Services;

public partial class ModsListClient(
    IOptions<GitLabOptions> options,
    ILogger<ModsListClient> logger,
    IGitLabClient client) : IModsListClient
{
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

        LogMetadataDownloaded(logger, path, projectId);

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

    public async Task<YamlStream?> GetYamlAsync(
        ProjectId projectId,
        string path,
        string branch = "HEAD",
        CancellationToken cancellationToken = default)
    {
        var files = client.GetRepository(projectId).Files;

        if (await files.FileExistsAsync(path, branch, cancellationToken))
        {
            var yamlStream = new YamlStream();
            Task ParseYaml(Stream stream)
            {
                yamlStream.Load(new StreamReader(stream));
                return Task.CompletedTask;
            }

            await files.GetRawAsync(
                    path,
                    ParseYaml,
                    new() { Ref = branch },
                    cancellationToken);

            return yamlStream;
        }

        return null;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Downloaded metadata file {MetadataPath} for project {ProjectId}")]
    private static partial void LogMetadataDownloaded(ILogger logger, string metadataPath, ProjectId projectId);
}
