using System.Net.Http.Json;
using System.Text.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;

namespace RaphCare.Client.Services;

public sealed class EmailAuthService(IHttpClientFactory httpClientFactory) : IEmailAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Response<EmailAuthResult>> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
        using var response = await client.PostAsJsonAsync(
            "api/auth/email/register",
            new { firstName, lastName, email, password },
            cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            return Response<EmailAuthResult>.Success(new EmailAuthResult
            {
                Success = dto?.Success ?? false,
                Token = dto?.Token
            });
        }

        var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
        return Response<EmailAuthResult>.Failure(error, (int)response.StatusCode);
    }

    public async Task<Response<EmailAuthResult>> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
        using var response = await client.PostAsJsonAsync(
            "api/auth/email/signin",
            new { email, password },
            cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            return Response<EmailAuthResult>.Success(new EmailAuthResult
            {
                Success = dto?.Success ?? false,
                Token = dto?.Token
            });
        }

        var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
        return Response<EmailAuthResult>.Failure(error, (int)response.StatusCode);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = response.Content is null ? null : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var dto = JsonSerializer.Deserialize<ErrorDto>(body, JsonOptions);
                if (!string.IsNullOrWhiteSpace(dto?.Error))
                    return dto.Error;
            }
            catch
            {
            }
        }

        return response.ReasonPhrase ?? "Request failed.";
    }

    private sealed class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
    }

    private sealed class ErrorDto
    {
        public string? Error { get; set; }
    }
}
