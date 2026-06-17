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

    public static void Apply(this Mod mod, ModDetailsDto dto)
    {
        mod.Url = dto.Url;
        mod.ProjectId = dto.ProjectId;
        mod.Titles = MergeTitles(mod.Titles, dto.Titles);
        mod.PackageType = MapToEntity(dto.PackageType);
        mod.Stars = dto.Stars;
        mod.ModCategories = [.. dto.Categories.Select(id => new ModToCategory() { CategoryId = id })];
        mod.Dependencies = [.. dto.Dependencies.Select(id => new ModDependency { DependencyModId = id })];
        mod.PreviewUrl = dto.PreviewUrl;
        mod.Author = dto.Author;
        mod.Metadata = dto.Metadata;
        mod.Versions = MergeVersions(mod.Versions, dto.Versions);
    }

    public static List<ModTitle> MergeTitles(ICollection<ModTitle> titles, ICollection<string> reloadedTitles)
    {
        var existingTitleIds = titles.ToDictionary(t => t.Title, t => t.Id);

        return [.. reloadedTitles
            .Select(title =>
            {
                if (!existingTitleIds.TryGetValue(title, out int id))
                {
                    id = default;
                }

                return new ModTitle
                {
                    Id = id,
                    Title = title
                };
            })];
    }

    public static List<ModVersion> MergeVersions(ICollection<ModVersion> versions, ICollection<ModVersionDto> reloadedVersions)
    {
        var existingVersionIds = versions.ToDictionary(t => t.Version, t => t.Id);

        return [.. reloadedVersions
            .Select(version =>
            {
                var versionEntity = version.ToEntity();

                if (existingVersionIds.TryGetValue(versionEntity.Version, out int id))
                {
                    versionEntity.Id = id;
                }

                return versionEntity;
            })];
    }

    public static PackageType MapToEntity(string type)
    {
        return TypeToEntity.TryGetValue(type, out var packageType)
            ? packageType
            : throw new NotSupportedException($"Unknown package type: ${type}");
    }
}
