namespace GitGudModsListLoader.Services;

public class ModAlreadyExistsException(long projectId)
    : Exception($"Mod for project {projectId} already exists.")
{
    public long ProjectId { get; } = projectId;
}