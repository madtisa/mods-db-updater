using GitGudModsListLoader.Models;
using GitGudModsListLoader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GitGudModsListLoader.Controllers;

// TODO: Add categories controller.
// TODO: Add versions controller.
[ApiController]
[Route("mods")]
public class ModsController(
    ILogger<ModsController> logger,
    IModsListService modsListService) : ControllerBase
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
    /// Update mod details from gitgud.
    /// </summary>
    /// <param name="id">Mod ID</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>200 - if succeded</returns>
    [Authorize]
    [HttpPost("{id}/update")]
    public async Task<ActionResult> Update(int id, CancellationToken token)
    {
        // TODO: Use access token and check ownership or just hardcode password for now
        // TODO: Move to policy.
        string? projectIdText = User.FindFirstValue("project_id");
        if (projectIdText is null || long.TryParse(projectIdText, out var projectId))
        {
            logger.LogError("Project id is invalid or missing in claims: '{ProjectId}'", projectIdText);
            return Forbid();
        }

        try
        {
            await modsListService.UpdateAsync(id, token);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound(new { projectId });
        }

        return Ok();
    }

    /// <summary>
    /// Add new mod and fetch its gitgud details
    /// </summary>
    /// <param name="request">New mod info to fetch from gitgud</param>
    /// <param name="token">Cancellation token</param>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> Add([FromBody] AddModRequest request, CancellationToken token)
    {
        await modsListService.AddAsync(null, request, token);
        return Created();
    }

    /// <summary>
    /// Add new mod with specific id and fetch its gitgud details
    /// </summary>
    /// <param name="request">New mod info to fetch from gitgud</param>
    /// <param name="token">Cancellation token</param>
    [Authorize]
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
