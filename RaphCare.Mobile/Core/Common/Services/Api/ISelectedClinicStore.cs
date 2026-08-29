namespace RaphCare.Mobile.Core.Common.Services.Api;

/// <summary>Persists the patient's chosen clinic for <c>X-Clinic-Id</c> on API calls.</summary>
public interface ISelectedClinicStore
{
    Guid? ClinicId { get; }
    string? ClinicName { get; }
    string? ReferenceCode { get; }

    void SetClinic(Guid clinicId, string name, string referenceCode);
    void Clear();
}
