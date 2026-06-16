using RaphCare.Domain.Identity;

namespace RaphCare.Application.Common.Interfaces;

public interface IEmailPasswordAuthService
{
    Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        Guid clinicId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
