using NGitLab.Models;

namespace GitGudModsListLoader.Services.VersionResolver;

public class ModPackageVersionResolver(IModsListClient client) : IVersionResolver
{
    public string PackageType => "mod-package";

    public IAsyncEnumerable<ModVersionInfo> ResolveAsync(ProjectId projectId)
    {
        return client.GetProjectReleasesAsync(projectId)
            .Select(release =>
                new ModVersionInfo(
                    release.TagName,
                    release.Commit.CommittedDate,
                    [..release.Assets.Links
                        // TODO: Add status of version ("invalid" when unable to parse)
                        .Where(link =>
                            link.LinkType == ReleaseLinkType.Package
                            && !string.IsNullOrEmpty(link.DirectAssetUrl))
                        .Select(link => link.DirectAssetUrl)]));
    }
}
