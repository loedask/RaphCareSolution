namespace RaphCare.Mobile.Core.Common.Configuration;

/// <summary>
/// Resolves the mobile API base URL. Release builds must not silently call localhost
/// (packaged TestHosting config should supply the hosted URL).
/// </summary>
public static class MobileApiBaseAddress
{
    /// <summary>Staging API used when Release config is missing or still points at loopback.</summary>
    public const string StagingDefault =
        "https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net/";

    public static string Resolve(string? configured, bool isDebugBuild)
    {
        var address = string.IsNullOrWhiteSpace(configured) ? string.Empty : configured.Trim();

        if (!isDebugBuild && (string.IsNullOrEmpty(address) || IsLoopback(address)))
            address = StagingDefault;

        if (string.IsNullOrEmpty(address))
            address = "http://localhost:5281/";

        return address.EndsWith('/') ? address : address + "/";
    }

    public static bool IsLoopback(string address)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
            return false;

        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("10.0.2.2", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("[::1]", StringComparison.OrdinalIgnoreCase);
    }
}
