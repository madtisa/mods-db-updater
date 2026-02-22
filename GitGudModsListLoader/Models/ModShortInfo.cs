namespace GitGudModsListLoader.Models;

public record ModShortInfo(int Id, string Title, long ProjectId, string MetadataPath = "src/meta.ini");
