using System.Text.Json;

namespace RaphCare.Client.Services.Base;

/// <summary>Shared JSON options for hand-written API calls (camelCase, case-insensitive).</summary>
internal static class ApiJson
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static byte[] SerializeToUtf8Bytes<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, Options);
}
