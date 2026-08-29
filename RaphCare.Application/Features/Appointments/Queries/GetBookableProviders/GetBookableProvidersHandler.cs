using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.DTOs;

namespace RaphCare.Application.Features.Appointments.Queries.GetBookableProviders;

public sealed class GetBookableProvidersHandler(
    IClinicContext clinicContext,
    IAdminClinicProviderQueryService providerQueryService)
    : IRequestHandler<GetBookableProvidersQuery, IReadOnlyList<BookableProviderDto>>
{
    public async Task<IReadOnlyList<BookableProviderDto>> Handle(
        GetBookableProvidersQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = clinicContext.ClinicId
            ?? throw new BusinessRuleException("Choose your clinic in Profile (My clinic), then try again.");

        var providers = await providerQueryService
            .GetProvidersAsync(clinicId, cancellationToken)
            .ConfigureAwait(false);

        return providers
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(p => new BookableProviderDto
            {
                Id = p.ProviderId,
                DisplayName = string.IsNullOrWhiteSpace(p.DisplayName) ? "Provider" : p.DisplayName.Trim()
            })
            .ToList();
    }
}
