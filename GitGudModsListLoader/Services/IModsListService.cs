using GitGudModsListLoader.Models;

namespace GitGudModsListLoader.Services;

public interface IModsListService
{
    IAsyncEnumerable<ModDto> ListAsync();
    
    Task<ModDto?> GetAsync(long projectId, CancellationToken token);
    
    Task AddAsync(AddModRequest request, CancellationToken token);
    
    Task UpdateAsync(long projectId, CancellationToken token);
}
