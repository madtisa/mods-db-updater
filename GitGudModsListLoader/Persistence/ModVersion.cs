using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Persistence;

[Index(nameof(ModId), nameof(Version), IsUnique = true)]
public class ModVersion
{
    public int Id { get; set; }

    public required string Version { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required string[] Urls { get; set; }

    public int ModId { get; set; }

    public Mod Mod { get; set; } = null!;
}
