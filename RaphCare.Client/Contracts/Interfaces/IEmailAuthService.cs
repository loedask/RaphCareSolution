using RaphCare.Client.Contracts;

namespace RaphCare.Client.Contracts.Interfaces;

public interface IEmailAuthService
{
    Task<Response<EmailAuthResult>> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Response<EmailAuthResult>> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}

public sealed class EmailAuthResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }
}
