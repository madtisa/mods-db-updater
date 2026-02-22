namespace GitGudModsListLoader.Services;

[Serializable]
public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException() { }
    public ProjectNotFoundException(long projectId) : base($"Project {projectId} is not found") { }
    public ProjectNotFoundException(string message) : base(message) { }
    public ProjectNotFoundException(string message, Exception inner) : base(message, inner) { }
}