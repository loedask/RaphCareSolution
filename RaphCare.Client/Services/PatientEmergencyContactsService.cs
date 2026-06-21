using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.EmergencyContacts;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientEmergencyContactsService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientEmergencyContactsService
{
    public async Task<Response<IReadOnlyList<PatientEmergencyContactViewModel>>> GetMyEmergencyContactsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<IReadOnlyList<EmergencyContactDto>>("api/patient/emergency-contacts", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<PatientEmergencyContactViewModel>>.Failure(result.ErrorMessage ?? "Could not load emergency contacts.", result.StatusCode);

        var list = (result.Data ?? Array.Empty<EmergencyContactDto>()).Select(Map).ToList();
        return Response<IReadOnlyList<PatientEmergencyContactViewModel>>.Success(list);
    }

    public async Task<Response<PatientEmergencyContactViewModel>> GetMyEmergencyContactAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<EmergencyContactDto>($"api/patient/emergency-contacts/{id}", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PatientEmergencyContactViewModel>.Failure(result.ErrorMessage ?? "Could not load emergency contact.", result.StatusCode);
        return Response<PatientEmergencyContactViewModel>.Success(Map(result.Data));
    }

    public async Task<Response<Guid>> AddEmergencyContactAsync(
        string name,
        string? relationship,
        string? phoneNumber,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var body = new { name, relationship, phoneNumber, email };
        var result = await PostAsync<CreatedGuidApiResponse>("api/patient/emergency-contacts", body, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Add failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<bool>> UpdateEmergencyContactAsync(
        Guid id,
        string name,
        string? relationship,
        string? phoneNumber,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var body = new { id, name, relationship, phoneNumber, email };
        var result = await PutNoContentAsync($"api/patient/emergency-contacts/{id}", body, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Update failed.", result.StatusCode);
    }

    public Task<Response<bool>> RemoveEmergencyContactAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/patient/emergency-contacts/{id}", cancellationToken);

    private static PatientEmergencyContactViewModel Map(EmergencyContactDto d) => new()
    {
        Id = d.Id,
        Name = d.Name ?? string.Empty,
        Relationship = d.Relationship,
        PhoneNumber = d.PhoneNumber,
        Email = d.Email,
    };

    private sealed class EmergencyContactDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Relationship { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
