using GitGudModsListLoader.Models;
using GitGudModsListLoader.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Services;

public class ModsListService(ILogger<ModsListService> logger, IModsScrubber modsScrubber, ModsDbContext context) : IModsListService
{
    public async Task UpdateAsync(long projectId, CancellationToken token)
    {
        var mod = await context.Mods
            .FirstOrDefaultAsync(mod => mod.ProjectId == projectId, token);

        if (mod is null)
        {
            logger.LogWarning("Mod for project {projectId} is missing", projectId);
            return;
        }

        var scrubModRequest = new ScrubModRequest(mod.ProjectId, mod.Titles.FirstOrDefault()?.Title, mod.MetadataPath);
        ModDto updatedMod = await modsScrubber.ScrubModDataAsync(scrubModRequest, token);

        mod.Apply(updatedMod);

        await context.SaveChangesAsync(token);
    }

    public Task<ModDto?> GetAsync(long projectId, CancellationToken token) =>
        context.Mods
            .AsNoTracking()
            .Select(ModDto.FromEntity)
            .FirstOrDefaultAsync(mod => mod.ProjectId == projectId, token);

    public IAsyncEnumerable<ModDto> ListAsync() =>
        context.Mods
            .AsNoTracking()
            .Select(ModDto.FromEntity)
            .AsAsyncEnumerable();

    public async Task AddAsync(AddModRequest request, CancellationToken token)
    {
        if (await context.Mods.AsNoTracking().AnyAsync(mod => mod.ProjectId == request.ProjectId, token))
        {
            throw new ModAlreadyExistsException(request.ProjectId);
        }

        var scrubModRequest = new ScrubModRequest(request.ProjectId, request.Title, request.MetadataPath);
        ModDto mod = await modsScrubber.ScrubModDataAsync(scrubModRequest, token);

        context.Mods.Add(mod.ToEntity());

        await context.SaveChangesAsync(token);
    }
}
