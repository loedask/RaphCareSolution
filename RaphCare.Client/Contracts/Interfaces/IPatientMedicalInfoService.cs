using RaphCare.Client.Contracts;
using RaphCare.Client.Models.MedicalInfo;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient-reported medical summary (<c>api/patient/medical-info</c>).</summary>
public interface IPatientMedicalInfoService
{
    Task<Response<MyPatientMedicalInfoViewModel>> GetMyMedicalInfoAsync(CancellationToken cancellationToken = default);

    Task<Response<bool>> UpdateMyMedicalInfoAsync(MyPatientMedicalInfoUpdateRequest request, CancellationToken cancellationToken = default);
}
