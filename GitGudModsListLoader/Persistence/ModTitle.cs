using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Persistence;

[Index(nameof(ModId), nameof(Title), IsUnique = true)]
public class ModTitle
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int ModId { get; set; }

    public Mod Mod { get; set; } = null!;
}
