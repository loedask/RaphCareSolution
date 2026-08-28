using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicProviders;

public sealed class GetAdminClinicProvidersQuery : IRequest<IReadOnlyList<AdminClinicProviderListItemDto>?>
{
    public Guid ClinicId { get; set; }
}
