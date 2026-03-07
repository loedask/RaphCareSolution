namespace RaphCare.Client.Services.Base;

/// <summary>
/// Partial class for extending the NSwag-generated API client.
/// </summary>

public partial class Client : IClient
{
    public HttpClient HttpClient
    {
        get
        {
            return _httpClient;
        }
    }
}
