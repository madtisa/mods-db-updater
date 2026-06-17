using GitGudModsListLoader.Models;
using GitGudModsListLoader.Services.VersionResolver;
using NGitLab.Models;
using YamlDotNet.RepresentationModel;

namespace GitGudModsListLoader.Services;

/// <summary>
/// Scrubs mod details from gitgud project
/// </summary>
public class GitGudModsScrubber(IModsListClient client, IVersionResolverRepository versionResolverRepository) : IModsScrubber
{
    private record struct WorkflowTitles(string? DisplayedModName, string? ModName);

    /// <summary>
    /// Scrubs mod details from gitgud project
    /// </summary>
    /// <param name="info">Information about gitgud project</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Mod details</returns>
    /// <exception cref="FormatException">Metadata has invalid format</exception>
    public async Task<ModDetailsDto> ScrubModDataAsync(ScrubModRequest info, CancellationToken token)
    {
        var workflowTitlesTask = GetWorkflowTitlesAsync(info.ProjectId, token);
        Project projectDetails = await client.GetProjectInfoAsync(info.ProjectId, token);

        var metadata = await client.GetModMetadataAsync(info.ProjectId, info.MetadataPath, token);
        var generalSection = metadata["General"]
            ?? throw new FormatException("Missing general section in metadata file");

        if (!metadata.TryGetValue("Plugins", out var pluginsSection)
            || !pluginsSection.TryGetValue("GitGud\\packageType", out var packageType)
            || string.IsNullOrWhiteSpace(packageType))
        {
            packageType = "mod-package";
        }

        HashSet<string> addedTitles = new(4, StringComparer.OrdinalIgnoreCase);
        List<string> titles = new(4);
        void AddTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title) || !addedTitles.Add(title))
            {
                return;
            }

            titles.Add(title);
        }

        var workflowTitles = await workflowTitlesTask;
        AddTitle(workflowTitles.DisplayedModName);

        generalSection.Remove("modName", out var title);
        AddTitle(title);

        AddTitle(info.Title);

        AddTitle(workflowTitles.ModName);

        generalSection.Remove("pictureUrl", out var previewUrl);
        previewUrl ??= projectDetails.AvatarUrl;

        generalSection.Remove("category", out var rawCategories);
        var modCategories = rawCategories is null
            ? []
            : rawCategories.Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);

        generalSection.Remove("dependencies", out var rawDependencies);
        var modDependencies = rawDependencies is null
            ? []
            : rawDependencies.Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);

        generalSection.Remove("author", out var author);

        generalSection.Remove("url", out var url);
        url ??= projectDetails.WebUrl;

        var versionResolver = versionResolverRepository.Get(packageType);
        var versions = await versionResolver
            .ResolveAsync(info.ProjectId)
            .ToListAsync(token);

        return new ModDetailsDto(
            default,
            url,
            info.ProjectId,
            titles,
            packageType,
            projectDetails.StarCount,
            [.. modCategories],
            [.. modDependencies],
            previewUrl,
            author,
            info.MetadataPath,
            generalSection,
            versions);
    }

    private async Task<WorkflowTitles> GetWorkflowTitlesAsync(ProjectId projectId, CancellationToken token)
    {
        YamlStream? workflow = await client.GetYamlAsync(projectId, ".gitlab-ci.yml", cancellationToken: token);
        if (workflow is null || workflow.Documents.Count <= 0)
        {
            return default;
        }

        var workflowRoot = (YamlMappingNode)workflow.Documents[0].RootNode;
        if (!workflowRoot.Children.TryGetValue("variables", out var workflowVariablesNode))
        {
            return default;
        }

        var variablesMapping = (YamlMappingNode)workflowVariablesNode;
        WorkflowTitles titles = default;

        if (
            variablesMapping.Children.TryGetValue("DISPLAYED_MOD_NAME", out var displayedModName) &&
            displayedModName is not null)
        {
            titles.DisplayedModName = displayedModName.ToString();
        }

        if (
            variablesMapping.Children.TryGetValue("MOD_NAME", out var modName) &&
            modName is not null)
        {
            titles.ModName = modName.ToString();
        }

        return titles;
    }
}
