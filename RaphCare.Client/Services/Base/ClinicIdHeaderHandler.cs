using RaphCare.Client.Contracts;

namespace RaphCare.Client.Services.Base;

/// <summary>
/// Attaches <c>X-Clinic-Id</c> when the host registers <see cref="IClinicIdProvider"/>.
/// </summary>
public sealed class ClinicIdHeaderHandler(IClinicIdProvider clinicIdProvider) : DelegatingHandler
{
    public const string HeaderName = "X-Clinic-Id";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clinicId = clinicIdProvider.GetClinicId();
        if (clinicId is { } id && id != Guid.Empty)
            request.Headers.TryAddWithoutValidation(HeaderName, id.ToString());

        return base.SendAsync(request, cancellationToken);
    }
}
