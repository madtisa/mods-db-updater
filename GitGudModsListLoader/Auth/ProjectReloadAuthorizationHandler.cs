using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GitGudModsListLoader.Auth;

public sealed class ProjectReloadAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<ProjectReloadAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectReloadAccessRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        var projectId = httpContext.Request.RouteValues["projectId"]?.ToString();
        if (string.IsNullOrEmpty(projectId))
        {
            return Task.CompletedTask;
        }

        var claimedProjectId = context.User.FindFirstValue("project_id");
        if (projectId.Equals(claimedProjectId, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, $"Project id {projectId} is missing in claims"));
        }

        return Task.CompletedTask;
    }
}
