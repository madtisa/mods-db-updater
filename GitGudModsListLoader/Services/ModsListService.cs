using GitGudModsListLoader.Exceptions;
using GitGudModsListLoader.Models;
using GitGudModsListLoader.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Services;

public class ModsListService(ILogger<ModsListService> logger, IModsScrubber modsScrubber, ModsDbContext context) : IModsListService
{
    public async Task ReloadAsync(int id, CancellationToken token)
    {
        var mod = await context.Mods
            .Include(mod => mod.Titles)
            .Include(mod => mod.Versions)
            .Include(mod => mod.ModCategories)
            .Include(mod => mod.Dependencies)
            .FirstOrDefaultAsync(mod => mod.Id == id, token);

        if (mod is null)
        {
            logger.LogWarning("Mod with id {id} is missing", id);
            return;
        }

        await ReloadModAsync(mod, token);
    }


    public async Task ReloadProjectAsync(long projectId, CancellationToken token)
    {
        var mod = await context.Mods
            .Include(mod => mod.Titles)
            .Include(mod => mod.Versions)
            .Include(mod => mod.ModCategories)
            .Include(mod => mod.Dependencies)
            .FirstOrDefaultAsync(mod => mod.ProjectId == projectId, token);

        if (mod is null)
        {
            logger.LogWarning("Mod for project {projectId} is missing", projectId);
            return;
        }

        await ReloadModAsync(mod, token);
    }

    private async Task ReloadModAsync(Mod mod, CancellationToken token)
    {
        var scrubModRequest = new ScrubModRequest(mod.ProjectId, mod.Titles.FirstOrDefault()?.Title, mod.MetadataPath);
        ModDetailsDto updatedMod = await modsScrubber.ScrubModDataAsync(scrubModRequest, token);

        mod.Apply(updatedMod);

        await context.SaveChangesAsync(token);
    }

    public Task<ModDetailsDto?> GetAsync(int id, CancellationToken token) =>
        context.Mods
            .AsNoTracking()
            .Select(ModDetailsDto.FromEntity)
            .FirstOrDefaultAsync(mod => mod.Id == id, token);

    public IAsyncEnumerable<ModListItemDto> ListAsync() =>
        context.Mods
            .AsNoTracking()
            .Select(ModListItemDto.FromEntity)
            .AsAsyncEnumerable();

    public async Task AddAsync(int? id, AddModRequest request, CancellationToken token)
    {
        if (await context.Mods
            .AsNoTracking()
            .AnyAsync(mod => mod.ProjectId == request.ProjectId || mod.Id == id, token))
        {
            throw new ModAlreadyExistsException(request.ProjectId);
        }

        var scrubModRequest = new ScrubModRequest(request.ProjectId, request.Title, request.MetadataPath);
        ModDetailsDto mod = await modsScrubber.ScrubModDataAsync(scrubModRequest, token);

        var modEntity = mod.ToEntity();
        if (id.HasValue)
        {
            modEntity.Id = id.Value;
        }

        context.Mods.Add(modEntity);

        await context.SaveChangesAsync(token);

        if (id.HasValue)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                SELECT setval(
                    pg_get_serial_sequence('"Mods"', 'Id'),
                    COALESCE(MAX("Id"), 1)
                )
                FROM "Mods";
                """,
                token);
        }
    }
}
