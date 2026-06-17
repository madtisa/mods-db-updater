using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Persistence;

public class ModsDbContext(DbContextOptions<ModsDbContext> options) : DbContext(options)
{
    public DbSet<Mod> Mods { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ModDependency>()
            .HasOne(x => x.Mod)
            .WithMany(x => x.Dependencies)
            .HasForeignKey(x => x.ModId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ModDependency>()
            .HasOne(x => x.DependencyMod)
            .WithMany(x => x.RequiredBy)
            .HasForeignKey(x => x.DependencyModId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Mod>()
            .Property(x => x.PackageType)
            .HasConversion<string>();

        modelBuilder.Entity<Category>().HasData(GetKnownCategories());

        modelBuilder.Entity<User>().HasData(new User { Id = -1, ExternalId = 36695, Role = Role.Admin });

        base.OnModelCreating(modelBuilder);
    }

    private static List<Category> GetKnownCategories() => [
        new() { Id = 1, Name = "Animations"},
        new() { Id = 2, Name = "Armour"},
        new() { Id = 3, Name = "Audio"},
        new() { Id = 4, Name = "Cities", ParentId = 12},
        new() { Id = 5, Name = "Clothing"},
        new() { Id = 6, Name = "Collectables"},
        new() { Id = 7, Name = "Creatures, Mounts, & Vehicles"},
        new() { Id = 8, Name = "Factions"},
        new() { Id = 9, Name = "Gameplay"},
        new() { Id = 10, Name = "Body, Face, & Hair"},
        new() { Id = 11, Name = "Items"},
        new() { Id = 12, Name = "Locations"},
        new() { Id = 13, Name = "NPCs"},
        new() { Id = 14, Name = "Patches", ParentId = 24},
        new() { Id = 15, Name = "Quests"},
        new() { Id = 16, Name = "Races & Classes"},
        new() { Id = 17, Name = "UI"},
        new() { Id = 18, Name = "Visuals"},
        new() { Id = 19, Name = "Weapons", ParentId = 11},
        new() { Id = 20, Name = "Magic"},
        new() { Id = 21, Name = "Models & Textures"},
        new() { Id = 22, Name = "Skills & Levelling", ParentId = 9},
        new() { Id = 23, Name = "Player Homes"},
        new() { Id = 24, Name = "Bugfixes"},
        new() { Id = 25, Name = "Castles & Mansions", ParentId = 23},
        new() { Id = 26, Name = "Cheats"},
        new() { Id = 27, Name = "Combat", ParentId = 9},
        new() { Id = 28, Name = "Companions"},
        new() { Id = 29, Name = "Environment"},
        new() { Id = 30, Name = "Immersion"},
        new() { Id = 31, Name = "Landscape Changes"},
        new() { Id = 32, Name = "Mercantile"},
        new() { Id = 33, Name = "Modders resources"},
        new() { Id = 34, Name = "Stealth"},
        new() { Id = 35, Name = "Utilities"},
        new() { Id = 36, Name = "Weapon & Armour Sets", ParentId = 11},
        new() { Id = 37, Name = "Ammo", ParentId = 11},
        new() { Id = 38, Name = "Music"},
        new() { Id = 39, Name = "Voice"},
        new() { Id = 40, Name = "Character Presets"},
        new() { Id = 41, Name = "Jewelry", ParentId = 5},
        new() { Id = 42, Name = "Backpacks", ParentId = 5},
        new() { Id = 43, Name = "Crafting", ParentId = 9},
        new() { Id = 44, Name = "Equipment", ParentId = 43},
        new() { Id = 45, Name = "Home/Settlement", ParentId = 43},
        new() { Id = 46, Name = "Shader Presets"},
        new() { Id = 47, Name = "Miscellaneous"},
        new() { Id = 48, Name = "Overhauls", ParentId = 9 },
        new() { Id = 49, Name = "Perks", ParentId = 9 },
        new() { Id = 51, Name = "Settlements", ParentId = 23 },
        new() { Id = 52, Name = "Poses" },
        new() { Id = 53, Name = "Power Armor", ParentId = 2 },
        new() { Id = 54, Name = "Radio", ParentId = 9 },
        new() { Id = 55, Name = "Shouts", ParentId = 9 },
        new() { Id = 56, Name = "Tattoos", ParentId = 10 },
        new() { Id = 58, Name = "Weather & Lighting", ParentId = 9 }];
}
