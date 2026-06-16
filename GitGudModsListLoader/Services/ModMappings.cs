using GitGudModsListLoader.Persistence;

namespace GitGudModsListLoader.Services;

public static class ModMappings
{
    private static readonly Dictionary<string, PackageType> TypeToEntity = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mod-package"] = PackageType.ModPackage,
        ["source-repository"] = PackageType.SourceRepository,
        ["source-package"] = PackageType.SourcePackage
    };

    public static void Apply(this Mod mod, ModDto dto)
    {
        mod.Url = dto.Url;
        mod.ProjectId = dto.ProjectId;
        mod.Titles = [.. dto.Titles.Select(title => new ModTitle { Title = title })];
        mod.PackageType = MapToEntity(dto.PackageType);
        mod.Stars = dto.Stars;
        mod.ModCategories = [.. dto.Categories.Select(id => new ModToCategory() { CategoryId = id })];
        mod.Dependencies = [.. dto.Dependencies.Select(id => new ModDependency { DependencyModId = id })];
        mod.PreviewUrl = dto.PreviewUrl;
        mod.Author = dto.Author;
        mod.Metadata = dto.Metadata;
        mod.Versions = [.. dto.Versions.Select(version => version.ToEntity())];
    }

    public static PackageType MapToEntity(string type)
    {
        return TypeToEntity.TryGetValue(type, out var packageType)
            ? packageType
            : throw new NotSupportedException($"Unknown package type: ${type}");
    }
}
