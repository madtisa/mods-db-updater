using System.ComponentModel.DataAnnotations;

namespace GitGudModsListLoader;

public class ModsListOptions
{
    [Required]
    [Range(1, long.MaxValue)]
    public required long ProjectId { get; init; }

    [Required]
    public required string Branch { get; init; }
}

public class GitLabOptions
{
    [Required]
    public required string Host { get; init; }

    public required int RetryCount { get; init; }

    public required TimeSpan RetryInterval { get; init; }

    [Required]
    public required ModsListOptions ModsList { get; init; }

    [Required]
    public required string ApiToken { get; init; }

    public required string Audience { get; init; }
}
