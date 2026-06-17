using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Family;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient family members (<c>api/patient/family-members</c>). Implemented by <see cref="RaphCare.Client.Services.PatientFamilyMembersService"/> wrapping generated <c>IClient</c>.</summary>
public interface IPatientFamilyMembersService
{
    Task<Response<IReadOnlyList<PatientFamilyMemberViewModel>>> GetMyFamilyMembersAsync(CancellationToken cancellationToken = default);

    Task<Response<PatientFamilyMemberViewModel>> GetMyFamilyMemberAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Response<Guid>> AddFamilyMemberAsync(
        string firstName,
        string lastName,
        string relationship,
        DateTime? dateOfBirth,
        string? phoneNumber,
        string? email,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> UpdateFamilyMemberAsync(
        Guid id,
        string firstName,
        string lastName,
        string relationship,
        DateTime? dateOfBirth,
        string? phoneNumber,
        string? email,
        Guid? linkedPatientId,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> RemoveFamilyMemberAsync(Guid id, CancellationToken cancellationToken = default);
}
