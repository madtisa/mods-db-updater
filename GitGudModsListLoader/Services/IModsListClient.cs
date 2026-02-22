using GitGudModsListLoader.Models;
using NGitLab;
using NGitLab.Models;
using YamlDotNet.RepresentationModel;

namespace GitGudModsListLoader.Services;

public interface IModsListClient
{
    Task<ModsListInfo> GetModsListAsync(CancellationToken cancellationToken);
    Task<Project> GetProjectInfoAsync(ProjectId projectId, CancellationToken cancellationToken);
    IAsyncEnumerable<ReleaseInfo> GetProjectReleasesAsync(ProjectId projectId);
    IAsyncEnumerable<Tag> GetProjectTagsAsync(ProjectId projectId);
    Task<Dictionary<string, Dictionary<string, string>>> GetModMetadataAsync(ProjectId projectId, string path, CancellationToken cancellationToken);
    Uri GetCommitArchiveUrl(ProjectId projectId, Sha1 commit);
    Task<IEnumerable<ModInfo>> GetModsDbAsync(CancellationToken cancellationToken);
    Task UpdateModsDbAsync(IEnumerable<ModInfo> mods, CancellationToken cancellationToken);
    Task<YamlStream?> GetYamlAsync(
        ProjectId projectId,
        string path,
        string branch = "HEAD",
        CancellationToken cancellationToken = default);
}
