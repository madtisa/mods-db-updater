using GitGudModsListLoader.Persistence;
using GitGudModsListLoader.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NGitLab;
using NGitLab.Models;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace GitGudModsListLoader.Auth;

// TODO: Replace with OAuth 2.0 in future.
public sealed class GitLabAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationHeader = "X-GitLab-Token";

    private readonly IOptionsMonitor<GitLabOptions> _gitLabOptions;
    private readonly ModsDbContext _context;

    public GitLabAuthenticationHandler(
        IOptionsMonitor<GitLabOptions> gitLabOptions,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ModsDbContext context)
        : base(options, logger, encoder)
    {
        _gitLabOptions = gitLabOptions;
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthenticationHeader, out var authorizationHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authorizationHeaderValues.First();
        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }

        Session gitLabUser;

        try
        {
            var options = _gitLabOptions.CurrentValue;
            var client = new GitLabClient(
                options.Host,
                token,
                new RequestOptions(options.RetryCount, options.RetryInterval)
                {
                    HttpClientTimeout = options.Timeout,
                });

            gitLabUser = await client.Users.GetCurrentUserAsync();
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid GitGud token");
        }

        var user = await _context.Users
            .SingleOrDefaultAsync(x => x.ExternalId == gitLabUser.Id);

        if (user == null)
        {
            return AuthenticateResult.Fail("User not registered");
        }

        var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new("gitlab_user_id", gitLabUser.Id.ToString()),
                    new(ClaimTypes.Name, gitLabUser.Username),
                    new(ClaimTypes.Role, user.Role.ToString())
                };

        var identity = new ClaimsIdentity(claims, Scheme.Name);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}