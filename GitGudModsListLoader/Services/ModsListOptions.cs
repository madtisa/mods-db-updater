using System.ComponentModel.DataAnnotations;

namespace GitGudModsListLoader.Services;

public class ModsListOptions
{
    [Required]
    [Range(1, long.MaxValue)]
    public required long ProjectId { get; init; }

    [Required]
    public required string Branch { get; init; }
}
