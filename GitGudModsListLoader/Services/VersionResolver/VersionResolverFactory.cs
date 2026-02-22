namespace GitGudModsListLoader.Services.VersionResolver;

public class VersionResolverRepository : IVersionResolverRepository
{
    private readonly Dictionary<string, IVersionResolver> _resolvers;

    public VersionResolverRepository(IEnumerable<IVersionResolver> resolvers)
    {
        var dict = new Dictionary<string, IVersionResolver>(StringComparer.OrdinalIgnoreCase);

        foreach (var resolver in resolvers)
        {
            if (!dict.TryAdd(resolver.PackageType, resolver))
            {
                throw new InvalidOperationException(
                    $"Duplicate resolver for package type '{resolver.PackageType}'.");
            }
        }

        _resolvers = dict;
    }

    public IVersionResolver Get(string packageType)
    {
        if (!_resolvers.TryGetValue(packageType, out var resolver))
        {
            throw new NotSupportedException($"Package type {packageType} is not supported");
        }

        return resolver;
    }
}
