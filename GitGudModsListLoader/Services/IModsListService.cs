using GitGudModsListLoader.Models;

namespace GitGudModsListLoader.Services;

public interface IModsListService
{
    IAsyncEnumerable<ModListItemDto> ListAsync();
    
    Task<ModDetailsDto?> GetAsync(int id, CancellationToken token);
    
    Task AddAsync(int? id, AddModRequest request, CancellationToken token);
    
    Task UpdateAsync(int id, CancellationToken token);
}
