using GitGudModsListLoader.Models;

namespace GitGudModsListLoader.Services;

public interface IModsScrubber
{
    Task<ModDto> ScrubModDataAsync(ScrubModRequest info, CancellationToken token);
}