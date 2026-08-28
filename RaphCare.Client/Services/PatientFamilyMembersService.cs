using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.Family;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientFamilyMembersService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientFamilyMembersService
{
    public async Task<Response<IReadOnlyList<PatientFamilyMemberViewModel>>> GetMyFamilyMembersAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<IReadOnlyList<FamilyMemberDto>>("api/patient/family-members", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<PatientFamilyMemberViewModel>>.Failure(result.ErrorMessage ?? "Could not load family members.", result.StatusCode);

        var list = (result.Data ?? Array.Empty<FamilyMemberDto>()).Select(Map).ToList();
        return Response<IReadOnlyList<PatientFamilyMemberViewModel>>.Success(list);
    }

    public async Task<Response<PatientFamilyMemberViewModel>> GetMyFamilyMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<FamilyMemberDto>($"api/patient/family-members/{id}", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PatientFamilyMemberViewModel>.Failure(result.ErrorMessage ?? "Could not load family member.", result.StatusCode);
        return Response<PatientFamilyMemberViewModel>.Success(Map(result.Data));
    }

    public async Task<Response<Guid>> AddFamilyMemberAsync(
        string firstName,
        string lastName,
        string relationship,
        DateTime? dateOfBirth,
        string? phoneNumber,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            firstName,
            lastName,
            relationship,
            dateOfBirth,
            phoneNumber,
            email,
            linkedPatientId = (Guid?)null
        };
        var result = await PostAsync<CreatedGuidApiResponse>("api/patient/family-members", body, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Add failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<bool>> UpdateFamilyMemberAsync(
        Guid id,
        string firstName,
        string lastName,
        string relationship,
        DateTime? dateOfBirth,
        string? phoneNumber,
        string? email,
        Guid? linkedPatientId,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            id,
            firstName,
            lastName,
            relationship,
            dateOfBirth,
            phoneNumber,
            email,
            linkedPatientId
        };
        var result = await PutNoContentAsync($"api/patient/family-members/{id}", body, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Update failed.", result.StatusCode);
    }

    public Task<Response<bool>> RemoveFamilyMemberAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/patient/family-members/{id}", cancellationToken);

    private static PatientFamilyMemberViewModel Map(FamilyMemberDto d) => new()
    {
        Id = d.Id,
        FirstName = d.FirstName ?? string.Empty,
        LastName = d.LastName ?? string.Empty,
        Relationship = d.Relationship ?? string.Empty,
        DateOfBirth = d.DateOfBirth,
        PhoneNumber = d.PhoneNumber,
        Email = d.Email,
        LinkedPatientId = d.LinkedPatientId
    };

    private sealed class FamilyMemberDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Relationship { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public Guid? LinkedPatientId { get; set; }
    }
}
