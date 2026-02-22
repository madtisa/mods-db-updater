namespace GitGudModsListLoader.Services;

public record ModVersionInfo(
    string Version,
    DateTime CreatedAt,
    string[] Urls);
