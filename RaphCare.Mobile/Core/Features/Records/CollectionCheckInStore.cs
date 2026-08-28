namespace RaphCare.Mobile.Core.Features.Records;

/// <summary>Remembers which hospital wall poster the patient last scanned.</summary>
public sealed class CollectionCheckInStore
{
    public Guid? ClinicId { get; private set; }

    public event Action? Changed;

    public void SetClinic(Guid? clinicId)
    {
        ClinicId = clinicId;
        Changed?.Invoke();
    }
}
