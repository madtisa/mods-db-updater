namespace GitGudModsListLoader.Services;

public record ModInfo(
    int Id,
    int ProjectId,
    string Title,
    string PackageType,
    int Stars,
    IEnumerable<int> Categories,
    IEnumerable<int> Dependencies,
    string? PreviewUrl,
    string? Author,
    Dictionary<string, string> Metadata,
    List<ModVersionInfo> Versions);
