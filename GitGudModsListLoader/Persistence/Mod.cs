using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Persistence;

[Index(nameof(ProjectId), IsUnique = true)]
public class Mod
{
    public int Id { get; set; }

    public required string Url { get; set; }

    public long ProjectId { get; set; }

    public required ICollection<ModTitle> Titles { get; set; }

    public required PackageType PackageType { get; set; }

    public int Stars { get; set; }

    public string? PreviewUrl { get; set; }

    public string? Author { get; set; }

    public required string MetadataPath { get; set; }

    public required Dictionary<string, string> Metadata { get; set; }

    public required List<ModVersion> Versions { get; set; }

    public ICollection<ModToCategory> ModCategories { get; set; } = [];

    public ICollection<ModDependency> Dependencies { get; set; } = [];

    public ICollection<ModDependency> RequiredBy { get; set; } = [];
}
