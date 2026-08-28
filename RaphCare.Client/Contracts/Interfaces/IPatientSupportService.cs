using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Support;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient help &amp; support (<c>api/patient/support</c>).</summary>
public interface IPatientSupportService
{
    Task<Response<PatientSupportContentViewModel>> GetContentAsync(CancellationToken cancellationToken = default);

    Task<Response<Guid>> SubmitMessageAsync(
        SubmitPatientSupportMessageRequest request,
        CancellationToken cancellationToken = default);
}
