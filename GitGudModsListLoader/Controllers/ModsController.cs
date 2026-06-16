using GitGudModsListLoader.Models;
using GitGudModsListLoader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GitGudModsListLoader.Controllers;

// TODO: Add categories controller.

[ApiController]
[Route("mods")]
public class ModsController(
    ILogger<ModsController> logger,
    IModsListService modsListService) : ControllerBase
{
    [HttpGet]
    public IAsyncEnumerable<ModDto> List()
    {
        return modsListService.ListAsync();
    }

    [HttpGet("{projectId}")]
    public async Task<ActionResult<ModDto>> Get(
        [Range(1, int.MaxValue)] long projectId,
        CancellationToken token)
    {
        ModDto? mod = await modsListService.GetAsync(projectId, token);
        if (mod is null)
        {
            return NotFound(new { projectId });
        }

        return Ok(mod);
    }

    [Authorize]
    // TODO: pass projectId and authorize via token.
    [HttpPost("update")]
    public async Task<ActionResult> Update(CancellationToken token)
    {
        // TODO: Move to policy.
        string? projectIdText = User.FindFirstValue("project_id");
        if (projectIdText is null || long.TryParse(projectIdText, out var projectId))
        {
            logger.LogError("Project id is invalid or missing in claims: '{ProjectId}'", projectIdText);
            return Forbid();
        }

        try
        {
            await modsListService.UpdateAsync(projectId, token);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound(new { projectId });
        }

        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> Add([FromBody] AddModRequest request, CancellationToken token)
    {
        await modsListService.AddAsync(request, token);
        return Ok();
    }
}
