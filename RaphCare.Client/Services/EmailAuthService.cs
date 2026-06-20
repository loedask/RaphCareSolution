using System.Net.Http.Json;
using System.Text.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;

namespace RaphCare.Client.Services;

public sealed class EmailAuthService(IHttpClientFactory httpClientFactory) : IEmailAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Response<IReadOnlyList<RegistrationClinicItem>>> GetRegistrationClinicsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            var clinics = await client
                .GetFromJsonAsync<List<RegistrationClinicDto>>("api/auth/email/clinics", JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (clinics is null)
                return Response<IReadOnlyList<RegistrationClinicItem>>.Failure("Could not load clinics.");

            IReadOnlyList<RegistrationClinicItem> items = clinics
                .Select(c => new RegistrationClinicItem { Id = c.Id, Name = c.Name })
                .ToList();
            return Response<IReadOnlyList<RegistrationClinicItem>>.Success(items);
        }
        catch (HttpRequestException ex)
        {
            return Response<IReadOnlyList<RegistrationClinicItem>>.Failure(ex.Message);
        }
    }

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
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var dto = JsonSerializer.Deserialize<ErrorDto>(body, JsonOptions);
                if (!string.IsNullOrWhiteSpace(dto?.Error))
                    return dto.Error;

                if (dto?.Errors is { Count: > 0 })
                    return FormatValidationErrors(dto.Errors);

                if (!string.IsNullOrWhiteSpace(dto?.Detail))
                    return dto.Detail;
            }
            catch
            {
            }
        }

        return response.ReasonPhrase ?? "Request failed.";
    }

    private static string FormatValidationErrors(Dictionary<string, string[]> errors)
    {
        var messages = errors
            .SelectMany(pair => pair.Value)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .ToList();

        return messages.Count > 0
            ? string.Join(" ", messages)
            : "One or more validation failures have occurred.";
    }

    private sealed class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public bool RequiresVerification { get; set; }
    }

    private sealed class ErrorDto
    {
        public string? Error { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    private sealed class RegistrationClinicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
