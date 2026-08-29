using RaphCare.Client.Contracts;

namespace RaphCare.Web.Services;

/// <summary>Supplies <c>X-Clinic-Id</c> from the process-wide active clinic store.</summary>
public sealed class WebClinicIdProvider(WebActiveClinicIdStore store) : IClinicIdProvider
{
    public Guid? GetClinicId() => store.ClinicId;
}
