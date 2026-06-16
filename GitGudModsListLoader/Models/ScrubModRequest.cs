namespace GitGudModsListLoader.Models;

public record ScrubModRequest(long ProjectId, string? Title, string MetadataPath = "src/meta.ini");
