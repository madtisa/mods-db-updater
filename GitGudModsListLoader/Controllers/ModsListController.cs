using GitGudModsListLoader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GitGudModsListLoader.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class ModsListController(
    ILogger<ModsListController> logger,
    IModsListService modsListService) : ControllerBase
{
    [HttpGet]
    public IAsyncEnumerable<ModDto> List()
    {
        return modsListService.ListAsync();
    }

    [HttpGet]
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

    [HttpPost]
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

    // TODO: Move to background worker or allow only to admin.
    //[HttpPost]
    //public async Task<ActionResult> UpdateAll(CancellationToken token)
    //{
    //    string? projectId = User.FindFirstValue("project_id");
    //    string authorizedProjectId = gitLabOptions.Value.ModsList.ProjectId.ToString();
    //    if (projectId is null || !projectId.Equals(authorizedProjectId, StringComparison.Ordinal))
    //    {
    //        logger.LogError("Project id '{ProjectId}' is not allowed", projectId);
    //        return Forbid();
    //    }

    //    await modsListService.UpdateAsync(projectId, token);

    //    return Ok();
    //}
}
