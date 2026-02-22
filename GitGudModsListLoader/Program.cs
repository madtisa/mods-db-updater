using GitGudModsListLoader;
using GitGudModsListLoader.Services;
using GitGudModsListLoader.Services.VersionResolver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using NGitLab;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(typeof(Program).Assembly);
}

var gitGudConfiguration = builder.Configuration.GetSection("GitLab");

GitLabOptions gitLabOptions = gitGudConfiguration.Get<GitLabOptions>()
    ?? throw new ApplicationException("Unable to get gitlab settings");

builder.Services
    .AddOptions<GitLabOptions>()
    .Bind(gitGudConfiguration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthentication()
    .AddJwtBearer((options) =>
    {
        options.Authority = gitLabOptions.Host;
        options.Audience = gitLabOptions.Audience;
        options.RequireHttpsMetadata = true;

        options.Events = new JwtBearerEvents()
        {
            OnTokenValidated = (context) =>
            {
                string? projectId = context.Principal?.FindFirstValue("project_id");
                if (string.IsNullOrEmpty(projectId))
                {
                    context.Fail("Project id claim is missing");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddScoped<IGitLabClient>(provider =>
{
    var options = provider.GetRequiredService<IOptions<GitLabOptions>>().Value;
    return new GitLabClient(
            options.Host,
            options.ApiToken,
            new RequestOptions(options.RetryCount, options.RetryInterval));
});
builder.Services.AddScoped<IModsListClient, ModsListClient>();

builder.Services.AddScoped<IVersionResolver, ModPackageVersionResolver>();
builder.Services.AddScoped<IVersionResolver, SourcePackageVersionResolver>();
builder.Services.AddScoped<IVersionResolver, SourceRepositoryVersionResolver>();

builder.Services.AddScoped<IVersionResolverRepository, VersionResolverRepository>();

builder.Services.AddScoped<IModsListService, ModsListService>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
