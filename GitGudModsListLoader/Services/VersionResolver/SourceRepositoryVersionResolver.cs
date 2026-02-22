using NGitLab.Models;

namespace GitGudModsListLoader.Services.VersionResolver;

public class SourceRepositoryVersionResolver(IModsListClient client) : IVersionResolver
{
    public string PackageType => "source-repository";

    public IAsyncEnumerable<ModVersionInfo> ResolveAsync(ProjectId projectId)
    {
        return client.GetProjectTagsAsync(projectId)
            .Select(tag =>
                new ModVersionInfo(
                    tag.Name,
                    tag.Commit.CommittedDate,
                    [client.GetCommitArchiveUrl(projectId, tag.Commit.Id).ToString()]));
    }
}
