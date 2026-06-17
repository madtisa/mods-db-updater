using GitGudModsListLoader;
using GitGudModsListLoader.Persistence;
using GitGudModsListLoader.Services;
using GitGudModsListLoader.Services.VersionResolver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.IncludeXmlComments(Assembly.GetExecutingAssembly()));

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
var db = scope.ServiceProvider.GetRequiredService<ModsDbContext>();
await db.Database.MigrateAsync();

app.Run();
