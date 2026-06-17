using RaphCare.Domain.Identity;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Abstraction over OTP-based identity provisioning and patient linking.
/// Keeps application layer decoupled from EF Core and DbContexts.
/// </summary>
public interface IIdentityOtpProvisioningService
{
    /// <summary>
    /// Ensures an <see cref="ApplicationUser"/> and corresponding <see cref="Patient"/> exist for the given phone number,
    /// and that <see cref="Patient.ApplicationUserId"/> is linked to the user.
    /// </summary>
    /// <param name="phoneNumber">The phone number that passed OTP verification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user and patient pair.</returns>
    Task<(ApplicationUser user, Patient patient)> EnsureUserAndPatientForPhoneAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a login audit record.
    /// </summary>
    /// <param name="audit">The login audit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogLoginAttemptAsync(LoginAudit audit, CancellationToken cancellationToken = default);
}

