using GitGudModsListLoader.Exceptions;
using GitGudModsListLoader.Models;
using GitGudModsListLoader.Persistence;
using GitGudModsListLoader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GitGudModsListLoader.Controllers;

// TODO: Add categories controller.
// TODO: Add versions controller.
[ApiController]
[Route("mods")]
public class ModsController(IModsListService modsListService) : ControllerBase
{
    /// <summary>
    /// List all mods
    /// </summary>
    /// <returns>Basic info about all mods</returns>
    [HttpGet]
    public IAsyncEnumerable<ModListItemDto> List()
    {
        return modsListService.ListAsync();
    }

    /// <summary>
    /// Get mod details
    /// </summary>
    /// <param name="id">Mod ID</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Mod details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ModDetailsDto>> Get(
        [Range(1, int.MaxValue)] int id,
        CancellationToken token)
    {
        ModDetailsDto? mod = await modsListService.GetAsync(id, token);
        if (mod is null)
        {
            return NotFound(new { id });
        }

        return Ok(mod);
    }

    /// <summary>
    /// Reload mod details from gitgud.
    /// </summary>
    /// <param name="id">Mod ID</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>200 - if succeded</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/reload/")]
    public async Task<ActionResult> Reload(int id, CancellationToken token)
    {
        try
        {
            await modsListService.ReloadAsync(id, token);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound(new { id });
        }

        return Ok();
    }

    /// <summary>
    /// Add new mod and fetch its gitgud details
    /// </summary>
    /// <param name="request">New mod info to fetch from gitgud</param>
    /// <param name="token">Cancellation token</param>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Add([FromBody] AddModRequest request, CancellationToken token)
    {
        await modsListService.AddAsync(null, request, token);
        return Created();
    }

    /// <summary>
    /// Add new mod with specific id and fetch its gitgud details
    /// </summary>
    /// <param name="id">Mod ID</param>
    /// <param name="request">New mod info to fetch from gitgud</param>
    /// <param name="token">Cancellation token</param>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}")]
    public async Task<ActionResult> Add(
        [FromRoute] int id,
        [FromBody] AddModRequest request,
        CancellationToken token)
    {
        await modsListService.AddAsync(id, request, token);
        return Created();
    }
}
