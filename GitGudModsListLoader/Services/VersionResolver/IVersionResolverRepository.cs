namespace GitGudModsListLoader.Services.VersionResolver;

public interface IVersionResolverRepository
{
    IVersionResolver Get(string packageType);
}