using GitGudModsListLoader.Exceptions;
using GitGudModsListLoader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitGudModsListLoader.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController(IModsListService modsListService) : ControllerBase
{
    /// <summary>
    /// Reload mod details from gitgud by project id.
    /// </summary>
    /// <param name="projectId">Mod project ID</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>200 - if succeded</returns>
    [Authorize(Policy = "ProjectReloadAccess")]
    // TODO: Move to separate controller.
    [HttpPost("{projectId}/reload")]
    public async Task<ActionResult> ReloadProject(long projectId, CancellationToken token)
    {
        try
        {
            await modsListService.ReloadProjectAsync(projectId, token);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound(new { projectId });
        }

        return Ok();
    }
}
