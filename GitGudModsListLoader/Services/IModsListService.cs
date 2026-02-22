
namespace GitGudModsListLoader.Services;

public interface IModsListService
{
    Task<IEnumerable<ModInfo>> GetAllAsync(CancellationToken token);
    Task UpdateAsync(long projectId, CancellationToken token);
    Task UpdateAllAsync(CancellationToken token);
}