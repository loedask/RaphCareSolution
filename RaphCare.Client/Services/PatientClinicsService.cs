using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Clinics;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientClinicsService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientClinicsService
{
    public async Task<Response<IReadOnlyList<PatientLinkedClinicViewModel>>> GetMyLinkedClinicsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<List<LinkedClinicDto>>("api/patient/clinics", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<PatientLinkedClinicViewModel>>.Failure(
                result.ErrorMessage ?? "Could not load linked hospitals.",
                result.StatusCode);

        IReadOnlyList<PatientLinkedClinicViewModel> items = (result.Data ?? [])
            .Select(d => new PatientLinkedClinicViewModel
            {
                ClinicId = d.ClinicId,
                Name = d.Name ?? string.Empty,
                ReferenceCode = d.ReferenceCode ?? string.Empty,
                AccessKind = d.AccessKind ?? string.Empty,
                GrantedAt = d.GrantedAt
            })
            .ToList();
        return Response<IReadOnlyList<PatientLinkedClinicViewModel>>.Success(items);
    }

    private sealed class LinkedClinicDto
    {
        public Guid ClinicId { get; set; }
        public string? Name { get; set; }
        public string? ReferenceCode { get; set; }
        public string? AccessKind { get; set; }
        public DateTime? GrantedAt { get; set; }
    }
}
