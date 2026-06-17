using GitGudModsListLoader.Models;

namespace GitGudModsListLoader.Services;

public interface IModsScrubber
{
    Task<ModDetailsDto> ScrubModDataAsync(ScrubModRequest info, CancellationToken token);
}