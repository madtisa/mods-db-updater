using GitGudModsListLoader.Models;

namespace GitGudModsListLoader.Services;

public interface IModsListService
{
    IAsyncEnumerable<ModListItemDto> ListAsync();
    
    Task<ModDetailsDto?> GetAsync(int id, CancellationToken token);
    
    Task AddAsync(int? id, AddModRequest request, CancellationToken token);

    Task ReloadAsync(int id, CancellationToken token);

    Task ReloadAllAsync(CancellationToken token);

    Task ReloadProjectAsync(long projectId, CancellationToken token);
}
