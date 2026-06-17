using System.ComponentModel.DataAnnotations;

namespace GitGudModsListLoader.Services;

public class GitLabOptions
{
    [Required]
    public required string Host { get; init; }

    public required int RetryCount { get; init; } = 1;

    public required TimeSpan RetryInterval { get; init; }

    public required TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
    
    [Required]
    public required ModsListOptions ModsList { get; init; }

    [Required]
    public required string ApiToken { get; init; }

    public required string Audience { get; init; }
}
