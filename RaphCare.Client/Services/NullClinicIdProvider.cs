using RaphCare.Client.Contracts;

namespace RaphCare.Client.Services;

/// <summary>Default provider when the host does not configure a clinic id.</summary>
internal sealed class NullClinicIdProvider : IClinicIdProvider
{
    public Guid? GetClinicId() => null;
}
