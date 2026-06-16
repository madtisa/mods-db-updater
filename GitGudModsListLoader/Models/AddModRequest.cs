namespace GitGudModsListLoader.Models;

public record AddModRequest(long ProjectId, string? Title, string MetadataPath = "src/meta.ini");
