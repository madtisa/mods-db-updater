using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Persistence;

[PrimaryKey(nameof(ModId), nameof(DependencyModId))]
public class ModDependency
{
    public int ModId { get; set; }

    public Mod Mod { get; set; } = null!;

    public int DependencyModId { get; set; }

    public Mod DependencyMod { get; set; } = null!;
    
    // TODO: Add required and version constraints in future (and add id as primary key)
}
