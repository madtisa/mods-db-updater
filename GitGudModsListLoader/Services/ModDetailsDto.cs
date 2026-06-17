using GitGudModsListLoader.Persistence;
using System.Linq.Expressions;

namespace GitGudModsListLoader.Services;

public record ModDetailsDto(
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
    List<ModVersionDto> Versions)
{
    internal Mod ToEntity()
    {
        return new()
        {
            Id = Id,
            Author = Author,
            ModCategories = [.. Categories.Select(id => new ModToCategory() { CategoryId = id })],
            Dependencies = [.. Dependencies.Select(id => new ModDependency { DependencyModId = id })],
            MetadataPath = MetadataPath,
            Metadata = Metadata,
            PackageType = ModMappings.MapToEntity(PackageType),
            PreviewUrl = PreviewUrl,
            ProjectId = ProjectId,
            Stars = Stars,
            Titles = [.. Titles.Select(title => new ModTitle { Title = title })],
            Url = Url,
            Versions = [.. Versions.Select(version => version.ToEntity())]
        };
    }

    internal static readonly Expression<Func<Mod, ModDetailsDto>> FromEntity = static entity =>
        new ModDetailsDto(
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
                .ToList());
}
