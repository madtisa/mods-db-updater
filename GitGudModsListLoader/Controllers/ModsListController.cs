using GitGudModsListLoader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GitGudModsListLoader.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class ModsListController(
    ILogger<ModsListController> logger,
    IOptions<GitLabOptions> gitLabOptions,
    IModsListService modsListService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ModInfo>> Get(CancellationToken token)
    {
        // TODO: Get only appropriate project (use project id from claims)
        var modsList = await modsListService.GetAllAsync(token);
        return modsList;
    }

    [HttpPost]
    public async Task<ActionResult> Update(CancellationToken token)
    {
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
    public async Task<ActionResult> UpdateAll(CancellationToken token)
    {
        string? projectId = User.FindFirstValue("project_id");
        string authorizedProjectId = gitLabOptions.Value.ModsList.ProjectId.ToString();
        if (projectId is null || !projectId.Equals(authorizedProjectId, StringComparison.Ordinal))
        {
            logger.LogError("Project id '{ProjectId}' is not allowed", projectId);
            return Forbid();
        }

        await modsListService.UpdateAllAsync(token);
        return Ok();
    }
}
