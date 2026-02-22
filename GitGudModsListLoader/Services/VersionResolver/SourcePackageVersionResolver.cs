using NGitLab.Models;

namespace GitGudModsListLoader.Services.VersionResolver;

public class SourcePackageVersionResolver(IModsListClient client) : IVersionResolver
{
    public string PackageType => "source-package";

    public IAsyncEnumerable<ModVersionInfo> ResolveAsync(ProjectId projectId)
    {
        return client.GetProjectReleasesAsync(projectId)
            .Select(release =>
                new ModVersionInfo(
                    release.TagName,
                    release.Commit.CommittedDate,
                    [client.GetCommitArchiveUrl(projectId, release.Commit.Id).ToString()]));
    }
}
