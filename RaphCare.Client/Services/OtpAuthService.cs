using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;

namespace RaphCare.Client.Services;

/// <summary>Calls OTP endpoints with JSON so the verify JWT is returned. Complements the NSwag client (verify method discards 200 body).</summary>
public sealed class OtpAuthService(IHttpClientFactory httpClientFactory) : IOtpAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Response<bool>> SendOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Response<bool>.Failure("Phone number is required.");

        var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
        using var response = await client.PostAsJsonAsync(
            "api/auth/otp/send",
            new { phoneNumber = phoneNumber.Trim() },
            cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return Response<bool>.Success(true);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            return Response<bool>.Failure("Too many requests. Try again later.", (int)response.StatusCode);

        var body = response.Content is null ? null : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return Response<bool>.Failure(body ?? response.ReasonPhrase ?? "Send OTP failed.", (int)response.StatusCode);
    }

    public async Task<Response<OtpVerifyResult>> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Response<OtpVerifyResult>.Failure("Phone number is required.");
        if (string.IsNullOrWhiteSpace(code))
            return Response<OtpVerifyResult>.Failure("Code is required.");

        var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
        using var response = await client.PostAsJsonAsync(
            "api/auth/otp/verify",
            new { phoneNumber = phoneNumber.Trim(), code = code.Trim() },
            cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<VerifyOtpResponseDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            return Response<OtpVerifyResult>.Success(new OtpVerifyResult
            {
                Success = dto?.Success ?? false,
                Token = dto?.Token
            });
        }

        var err = response.Content is null ? null : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return Response<OtpVerifyResult>.Failure(err ?? response.ReasonPhrase ?? "Verification failed.", (int)response.StatusCode);
    }

    private sealed class VerifyOtpResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
    }
}
