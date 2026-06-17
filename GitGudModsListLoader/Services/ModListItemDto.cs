using GitGudModsListLoader.Persistence;
using System.Linq.Expressions;

namespace GitGudModsListLoader.Services;

public record ModListItemDto(
    int Id,
    string Url,
    long ProjectId,
    ICollection<string> Titles,
    string PackageType,
    int Stars,
    ICollection<int> Categories,
    ICollection<int> Dependencies,
    string? PreviewUrl,
    string? Author,
    string MetadataPath,
    Dictionary<string, string> Metadata,
    ModVersionDto? LatestVersion)
{
    public static readonly Expression<Func<Mod, ModListItemDto>> FromEntity = static entity =>
        new ModListItemDto(
            entity.Id,
            entity.Url,
            entity.ProjectId,
            entity.Titles.Select(title => title.Title).ToList(),
            entity.PackageType == Persistence.PackageType.ModPackage ? "mod-package" :
                entity.PackageType == Persistence.PackageType.SourceRepository ? "source-repository" :
                entity.PackageType == Persistence.PackageType.SourcePackage ? "source-package" :
                "unknown",
            entity.Stars,
            entity.ModCategories.Select(category => category.CategoryId).ToList(),
            entity.Dependencies.Select(dep => dep.DependencyModId).ToList(),
            entity.PreviewUrl,
            entity.Author,
            entity.MetadataPath,
            entity.Metadata,
            entity.Versions
                .OrderByDescending(entity => entity.CreatedAt)
                .Select(entity => new ModVersionDto(entity.Version, entity.CreatedAt, entity.Urls))
                .FirstOrDefault());
}
