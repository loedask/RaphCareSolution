using System.Net.Http.Json;
using System.Text.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;

namespace RaphCare.Client.Services;

public sealed class EmailAuthService(IHttpClientFactory httpClientFactory) : IEmailAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Response<IReadOnlyList<RegistrationClinicItem>>> GetRegistrationClinicsAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            var uri = string.IsNullOrWhiteSpace(search)
                ? "api/auth/email/clinics"
                : $"api/auth/email/clinics?q={Uri.EscapeDataString(search.Trim())}";
            var clinics = await client
                .GetFromJsonAsync<List<RegistrationClinicDto>>(uri, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (clinics is null)
                return Response<IReadOnlyList<RegistrationClinicItem>>.Failure("Could not load clinics.");

            IReadOnlyList<RegistrationClinicItem> items = clinics
                .Select(MapClinic)
                .ToList();
            return Response<IReadOnlyList<RegistrationClinicItem>>.Success(items);
        }
        catch (HttpRequestException ex)
        {
            return Response<IReadOnlyList<RegistrationClinicItem>>.Failure(ex.Message);
        }
    }

    public async Task<Response<RegistrationClinicItem>> ResolveClinicByReferenceAsync(
        string referenceCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            var uri = $"api/auth/email/clinics/by-reference?code={Uri.EscapeDataString(referenceCode.Trim())}";
            using var response = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<RegistrationClinicItem>.Failure("No clinic found for that reference code.", 404);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                return Response<RegistrationClinicItem>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<RegistrationClinicDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<RegistrationClinicItem>.Failure("Could not resolve clinic.");

            return Response<RegistrationClinicItem>.Success(MapClinic(dto));
        }
        catch (HttpRequestException ex)
        {
            return Response<RegistrationClinicItem>.Failure(ex.Message);
        }
    }

    private static RegistrationClinicItem MapClinic(RegistrationClinicDto c) =>
        new()
        {
            Id = c.Id,
            Name = c.Name,
            ReferenceCode = c.ReferenceCode
        };

    public async Task<Response<object>> SendEmailVerificationAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync("api/auth/email/send-verification", new { email }, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<object>.Success(new object());

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return Response<object>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            return Response<object>.Failure(ex.Message);
        }
    }

    public async Task<Response<object>> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync("api/auth/email/forgot-password", new { email }, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<object>.Success(new object());

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return Response<object>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            return Response<object>.Failure(ex.Message);
        }
    }

    public async Task<Response<EmailAuthResult>> ConfirmPasswordResetAsync(
        string email,
        string verificationCode,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAuthAsync(
                    "api/auth/email/reset-password",
                    new { email, verificationCode, newPassword },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return Response<EmailAuthResult>.Failure(ex.Message);
        }
    }

    public async Task<Response<EmailAuthResult>> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        Guid? clinicId = null,
        string? verificationCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAuthAsync(
                    "api/auth/email/register",
                    new { firstName, lastName, email, password, clinicId, verificationCode },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return Response<EmailAuthResult>.Failure(ex.Message);
        }
    }

    public async Task<Response<EmailAuthResult>> SignInAsync(
        string email,
        string password,
        string? verificationCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAuthAsync(
                    "api/auth/email/signin",
                    new { email, password, verificationCode },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return Response<EmailAuthResult>.Failure(ex.Message);
        }
    }

    public async Task<Response<EmailAuthResult>> RegisterProfessionalAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string? verificationCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAuthAsync(
                    "api/auth/email/register-professional",
                    new { firstName, lastName, email, password, verificationCode },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return Response<EmailAuthResult>.Failure(ex.Message);
        }
    }

    public async Task<Response<EmailAuthResult>> SignInProfessionalAsync(
        string email,
        string password,
        string? verificationCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAuthAsync(
                    "api/auth/email/signin-professional",
                    new { email, password, verificationCode },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return Response<EmailAuthResult>.Failure(ex.Message);
        }
    }

    private async Task<Response<EmailAuthResult>> PostAuthAsync(string path, object body, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
        using var response = await client.PostAsJsonAsync(path, body, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            return Response<EmailAuthResult>.Success(new EmailAuthResult
            {
                Success = dto?.Success ?? false,
                Token = dto?.Token,
                RequiresVerification = dto?.RequiresVerification ?? false
            });
        }

        var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
        return Response<EmailAuthResult>.Failure(error, (int)response.StatusCode);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = response.Content is null ? null : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return RaphCare.Client.Services.Base.ApiErrorMessages.FromBody(body, response.StatusCode);
    }

    private sealed class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public bool RequiresVerification { get; set; }
    }

    private sealed class RegistrationClinicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReferenceCode { get; set; } = string.Empty;
    }
}
