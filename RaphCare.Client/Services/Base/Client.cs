using System.Text.Json;

namespace RaphCare.Client.Services.Base;

/// <summary>
/// Partial class for extending the NSwag-generated API client. Exposes HttpClient for BaseHttpService.
/// </summary>
public partial class Client : IClient
{
    public HttpClient HttpClient => _httpClient;

    /// <summary>UTF-8 JSON bytes using the same options as generated POST bodies (for HMAC before <see cref="IClient.EventsAsync"/>).</summary>
    public byte[] SerializeRequestBodyToUtf8Bytes<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerSettings);
}
