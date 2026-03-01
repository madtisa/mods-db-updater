
namespace GitGudModsListLoader.Services;

public interface IModsListService
{
    Task<ModInfo?> GetAsync(long projectId, CancellationToken token);
    Task<IEnumerable<ModInfo>> GetAllAsync(CancellationToken token);
    Task UpdateAsync(long projectId, CancellationToken token);
    Task UpdateAllAsync(CancellationToken token);
}