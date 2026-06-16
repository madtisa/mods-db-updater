using NGitLab.Models;

namespace GitGudModsListLoader.Services.VersionResolver;

public interface IVersionResolver
{
    string PackageType { get; }
    
    IAsyncEnumerable<ModVersionDto> ResolveAsync(ProjectId projectId);
}
