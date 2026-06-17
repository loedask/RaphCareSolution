using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Insurance;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient insurance API (<c>api/patient/insurance/...</c>) via generated <see cref="Services.Base.IClient"/>.</summary>
public interface IPatientInsuranceService
{
    Task<Response<IReadOnlyList<InsurancePlanOptionViewModel>>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<Response<PagedPatientInsuranceProfilesViewModel>> GetMyProfilesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Response<PatientInsuranceProfileViewModel?>> GetMyProfileAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateProfileAsync(CreatePatientInsuranceProfileRequest request, CancellationToken cancellationToken = default);
    Task<Response<bool>> UpdateProfileAsync(UpdatePatientInsuranceProfileRequest request, CancellationToken cancellationToken = default);
}
