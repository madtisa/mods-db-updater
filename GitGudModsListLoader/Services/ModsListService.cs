using GitGudModsListLoader.Models;
using GitGudModsListLoader.Services.VersionResolver;
using NGitLab.Models;

namespace GitGudModsListLoader.Services;

public class ModsListService(
    IModsListClient client,
    IVersionResolverRepository versionResolverRepository) : IModsListService
{
    public async Task UpdateAsync(long projectId, CancellationToken token)
    {
        ModInfo modInfo = await GetAsync(projectId, token)
            ?? throw new ProjectNotFoundException(projectId);

        IEnumerable<ModInfo> modsDb = await client.GetModsDbAsync(token);
        Dictionary<int, ModInfo> modsDbMap = modsDb.ToDictionary(mod => mod.Id);

        modsDbMap[modInfo.Id] = modInfo;

        var updatedModsDb = modsDbMap.Values.OrderBy(mod => mod.Id);
        await client.UpdateModsDbAsync(updatedModsDb, token);
    }

    public async Task UpdateAllAsync(CancellationToken token)
    {
        IEnumerable<ModInfo> modsDb = await GetAllAsync(token);
        await client.UpdateModsDbAsync(modsDb, token);
    }

    public async Task<ModInfo?> GetAsync(long projectId, CancellationToken token)
    {
        ModsListInfo modsList = await client.GetModsListAsync(token);

        var mod = modsList.Mods.FirstOrDefault(mod => mod.ProjectId == projectId);
        if (mod is null)
        {
            return null;
        }

        return await GetModDataAsync(mod, token);
    }

    public async Task<IEnumerable<ModInfo>> GetAllAsync(CancellationToken token)
    {
        ModsListInfo modsList = await client.GetModsListAsync(token);

        // TODO: Move throttling number to config.
        var throttle = new SemaphoreSlim(6);

        var query = modsList.Mods
            .Select(async (mod) =>
            {
                await throttle.WaitAsync(token);

                try
                {
                    return await GetModDataAsync(mod, token);
                }
                finally
                {
                    throttle.Release();
                }
            })
            .ToList();

        return await Task.WhenAll(query);
    }

    private async Task<ModInfo> GetModDataAsync(ModShortInfo info, CancellationToken token)
    {
        Project projectDetails = await client.GetProjectInfoAsync(info.ProjectId, token);

        var metadata = await client.GetModMetadataAsync(info.ProjectId, info.MetadataPath, token);
        var generalSection = metadata["General"]
            ?? throw new FormatException("Missing general section in metadata file");

        if (!metadata.TryGetValue("Plugins", out var pluginsSection)
            || !pluginsSection.TryGetValue("GitGud\\packageType", out var packageType)
            || string.IsNullOrWhiteSpace(packageType))
        {
            packageType = "mod-package";
        }

        generalSection.Remove("modName", out var title);
        title ??= info.Title;

        generalSection.Remove("pictureUrl", out var previewUrl);
        previewUrl ??= projectDetails.AvatarUrl;

        generalSection.Remove("category", out var rawCategories);
        var modCategories = rawCategories is null
            ? []
            : rawCategories.Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);

        generalSection.Remove("dependencies", out var rawDependencies);
        var modDependencies = rawDependencies is null
            ? []
            : rawDependencies.Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);

        generalSection.Remove("author", out var author);

        var versionResolver = versionResolverRepository.Get(packageType);
        var versions = await versionResolver
            .ResolveAsync(info.ProjectId)
            .ToListAsync(token);

        return new ModInfo(
            info.Id,
            info.ProjectId,
            title,
            packageType,
            projectDetails.StarCount,
            modCategories,
            modDependencies,
            previewUrl,
            author,
            generalSection,
            versions);
    }
}
