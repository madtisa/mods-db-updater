using NGitLab.Models;

namespace GitGudModsListLoader.Services.VersionResolver;

public class SourcePackageVersionResolver(IModsListClient client) : IVersionResolver
{
    public string PackageType => "source-package";

    public IAsyncEnumerable<ModVersionDto> ResolveAsync(ProjectId projectId)
    {
        return client.GetProjectReleasesAsync(projectId)
            .Select(release =>
                new ModVersionDto(
                    release.TagName,
                    release.Commit.CommittedDate,
                    [client.GetCommitArchiveUrl(projectId, release.Commit.Id).ToString()]));
    }
}
