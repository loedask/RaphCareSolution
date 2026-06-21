using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.MedicalInfo;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientMedicalInfoService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientMedicalInfoService
{
    public async Task<Response<MyPatientMedicalInfoViewModel>> GetMyMedicalInfoAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<MedicalInfoDto>("api/patient/medical-info", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<MyPatientMedicalInfoViewModel>.Failure(result.ErrorMessage ?? "Could not load medical information.", result.StatusCode);
        return Response<MyPatientMedicalInfoViewModel>.Success(Map(result.Data));
    }

    public async Task<Response<bool>> UpdateMyMedicalInfoAsync(MyPatientMedicalInfoUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await PutNoContentAsync("api/patient/medical-info", request, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Update failed.", result.StatusCode);
    }

    private static MyPatientMedicalInfoViewModel Map(MedicalInfoDto d) => new()
    {
        BloodType = d.BloodType ?? string.Empty,
        Allergies = d.Allergies ?? string.Empty,
        ChronicConditions = d.ChronicConditions ?? string.Empty,
        Medications = d.Medications ?? string.Empty,
        PrimaryDoctor = d.PrimaryDoctor ?? string.Empty,
    };

    private sealed class MedicalInfoDto
    {
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? Medications { get; set; }
        public string? PrimaryDoctor { get; set; }
    }
}
