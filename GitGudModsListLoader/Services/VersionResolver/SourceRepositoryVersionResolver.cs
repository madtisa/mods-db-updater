using NGitLab.Models;

namespace GitGudModsListLoader.Services.VersionResolver;

public class SourceRepositoryVersionResolver(IModsListClient client) : IVersionResolver
{
    public string PackageType => "source-repository";

    public IAsyncEnumerable<ModVersionDto> ResolveAsync(ProjectId projectId)
    {
        return client.GetProjectTagsAsync(projectId)
            .Select(tag =>
                new ModVersionDto(
                    tag.Name,
                    tag.Commit.CommittedDate,
                    [client.GetCommitArchiveUrl(projectId, tag.Commit.Id).ToString()]));
    }
}
