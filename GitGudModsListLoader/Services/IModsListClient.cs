using NGitLab;
using NGitLab.Models;
using YamlDotNet.RepresentationModel;

namespace GitGudModsListLoader.Services;

public interface IModsListClient
{
    Task<Project> GetProjectInfoAsync(ProjectId projectId, CancellationToken cancellationToken);
    
    IAsyncEnumerable<ReleaseInfo> GetProjectReleasesAsync(ProjectId projectId);
    
    IAsyncEnumerable<Tag> GetProjectTagsAsync(ProjectId projectId);
    
    Task<Dictionary<string, Dictionary<string, string>>> GetModMetadataAsync(ProjectId projectId, string path, CancellationToken cancellationToken);
    
    Uri GetCommitArchiveUrl(ProjectId projectId, Sha1 commit);
    
    Task<YamlStream?> GetYamlAsync(
        ProjectId projectId,
        string path,
        string branch = "HEAD",
        CancellationToken cancellationToken = default);
}
