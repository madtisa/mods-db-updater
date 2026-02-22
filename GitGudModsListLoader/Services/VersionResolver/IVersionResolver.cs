using NGitLab.Models;

namespace GitGudModsListLoader.Services.VersionResolver;

public interface IVersionResolver
{
    string PackageType { get; }
    IAsyncEnumerable<ModVersionInfo> ResolveAsync(ProjectId projectId);
}
