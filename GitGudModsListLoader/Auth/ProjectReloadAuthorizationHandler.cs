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

        var requestedProjectId = httpContext.Request.RouteValues["projectId"]?.ToString();
        if (string.IsNullOrEmpty(requestedProjectId))
        {
            return Task.CompletedTask;
        }

        var authorizedProjectId = context.User.FindFirstValue("project_id");
        if (requestedProjectId.Equals(authorizedProjectId, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, $"Project ID {requestedProjectId} is not allowed"));
        }

        return Task.CompletedTask;
    }
}
