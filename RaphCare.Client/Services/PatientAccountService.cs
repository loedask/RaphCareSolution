using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientAccountService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientAccountService
{
    public async Task<Response<object>> ChangePasswordAsync(
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var result = await PostNoContentAsync(
            "api/patient/account/change-password",
            new { currentPassword, newPassword },
            cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? Response<object>.Success(new object())
            : Response<object>.Failure(result.ErrorMessage ?? "Password change failed.", result.StatusCode);
    }
}
