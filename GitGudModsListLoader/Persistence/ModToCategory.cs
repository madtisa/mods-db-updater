using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Persistence;

[PrimaryKey(nameof(ModId), nameof(CategoryId))]
public class ModToCategory
{
    public int ModId { get; set; }
    public Mod Mod { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
