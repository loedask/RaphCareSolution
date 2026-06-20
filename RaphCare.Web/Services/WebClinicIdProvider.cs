using RaphCare.Client.Contracts;

namespace RaphCare.Web.Services;

public sealed class WebClinicIdProvider(IClinicContextService clinicContext) : IClinicIdProvider
{
    public Guid? GetClinicId() => clinicContext.CurrentClinicId;
}
