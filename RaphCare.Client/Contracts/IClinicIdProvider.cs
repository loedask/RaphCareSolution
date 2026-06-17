namespace RaphCare.Client.Contracts;

/// <summary>
/// Supplies the clinic (tenant) id sent as <c>X-Clinic-Id</c> on API requests.
/// </summary>
public interface IClinicIdProvider
{
    Guid? GetClinicId();
}
