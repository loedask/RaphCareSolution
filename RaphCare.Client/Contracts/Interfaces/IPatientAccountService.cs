namespace RaphCare.Client.Contracts.Interfaces;

public interface IPatientAccountService
{
    Task<Response<object>> ChangePasswordAsync(
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);
}
