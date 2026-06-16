using GitGudModsListLoader.Persistence;

namespace GitGudModsListLoader.Services;

public record ModVersionDto(
    string Version,
    DateTime CreatedAt,
    string[] Urls)
{
    public ModVersion ToEntity() =>
        new()
        {
            Version = Version,
            CreatedAt = CreatedAt,
            Urls = Urls,
        };
}
