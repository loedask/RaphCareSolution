namespace RaphCare.Portal.Services;

/// <summary>
/// Holds the active hospital id for <c>X-Clinic-Id</c>. Registered as a singleton so
/// UI and <see cref="RaphCare.Client.Services.Base.ClinicIdHeaderHandler"/> share one value
/// (HttpClient handlers do not reliably use scoped services).
/// </summary>
public sealed class WebActiveClinicIdStore
{
    private Guid? _clinicId;

    /// <summary>Current clinic id, or null when none is selected.</summary>
    public Guid? ClinicId
    {
        get => _clinicId;
        set => _clinicId = value is { } id && id != Guid.Empty ? id : null;
    }
}
