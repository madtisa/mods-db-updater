namespace GitGudModsListLoader.Models;

public record ModShortInfo(int Id, string Title, int ProjectId, string MetadataPath = "src/meta.ini");
