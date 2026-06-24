using GitGudModsListLoader.Auth;
using GitGudModsListLoader.Exceptions;
using GitGudModsListLoader.Persistence;
using GitGudModsListLoader.Services;
using GitGudModsListLoader.Services.VersionResolver;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NGitLab;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Security.Claims;



var builder = WebApplication.CreateBuilder(args);

var otel = builder.Services.AddOpenTelemetry();

otel.ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName));

otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql()
        .AddOtlpExporter();
});

otel.WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsqlInstrumentation()
        .AddRuntimeInstrumentation()
        // Metrics provides by ASP.NET Core in .NET 8
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
        // Metrics provided by System.Net libraries
        .AddMeter("System.Net.Http")
        .AddMeter("System.Net.NameResolution")
        .AddPrometheusExporter();
});

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

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Combined";
        options.DefaultChallengeScheme = "Combined";
    })
    .AddPolicyScheme("Combined", null, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            return context.Request.Headers.ContainsKey(GitLabAuthenticationHandler.AuthenticationHeader)
                ? "GitLab"
                : "Jwt";
        };
    })
    .AddJwtBearer("Jwt", (options) =>
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
    })
    .AddScheme<AuthenticationSchemeOptions, GitLabAuthenticationHandler>("GitLab", null);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAuthorizationHandler, ProjectReloadAuthorizationHandler>();

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy("ProjectReloadAccess", policy =>
    {
        policy.AddAuthenticationSchemes("Jwt");
        policy.AddAuthenticationSchemes("GitLab");

        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new ProjectReloadAccessRequirement());
    });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Assembly.GetExecutingAssembly());

    options.AddSecurityDefinition("GitLab", new OpenApiSecurityScheme
    {
        Name = GitLabAuthenticationHandler.AuthenticationHeader,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Scheme = "GitLab",
        BearerFormat = "JWT",
        Description = "Enter GitLab token",
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("GitLab", document)] = []
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<ModsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Mods"));
});

builder.Services.AddScoped<IGitLabClient>(provider =>
{
    var options = provider.GetRequiredService<IOptions<GitLabOptions>>().Value;
    var client = new GitLabClient(
            options.Host,
            options.ApiToken,
            new RequestOptions(options.RetryCount, options.RetryInterval)
            {
                HttpClientTimeout = options.Timeout,
            });
    return client;
});
builder.Services.AddScoped<IModsListClient, ModsListClient>();

builder.Services.AddScoped<IVersionResolver, ModPackageVersionResolver>();
builder.Services.AddScoped<IVersionResolver, SourcePackageVersionResolver>();
builder.Services.AddScoped<IVersionResolver, SourceRepositoryVersionResolver>();

builder.Services.AddScoped<IVersionResolverRepository, VersionResolverRepository>();

builder.Services.AddScoped<IModsScrubber, GitGudModsScrubber>();
builder.Services.AddScoped<IModsListService, ModsListService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.UseAuthorization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();

app.MapSwagger();
app.MapSwaggerUI();

using var scope = app.Services.CreateScope();
var _context = scope.ServiceProvider.GetRequiredService<ModsDbContext>();
await _context.Database.MigrateAsync();

app.Run();
