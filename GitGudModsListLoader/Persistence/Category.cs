using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Persistence;

[Index(nameof(Name), IsUnique = true)]
public class Category
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public int Order { get; set; }

    public int? ParentId { get; set; }

    public Category? Parent { get; set; }

    public ICollection<ModToCategory> ModCategories { get; set; } = [];
}
