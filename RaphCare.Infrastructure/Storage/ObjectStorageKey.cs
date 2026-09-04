namespace RaphCare.Infrastructure.Storage;

internal static class ObjectStorageKey
{
    public static string NormalizeContainer(string container)
    {
        var value = container.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(value)
            || value.Contains('/', StringComparison.Ordinal)
            || value.Contains('\\', StringComparison.Ordinal)
            || value.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException("Invalid storage container.", nameof(container));
        }

        return value;
    }

    public static string NormalizeKey(string key)
    {
        var parts = key.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts.Any(part => part == "." || part == ".." || part.Contains(':')))
            throw new ArgumentException("Invalid storage key.", nameof(key));

        return string.Join('/', parts);
    }
}
