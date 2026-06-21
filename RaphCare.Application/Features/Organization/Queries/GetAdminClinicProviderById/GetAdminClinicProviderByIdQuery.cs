using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicProviderById;

public sealed class GetAdminClinicProviderByIdQuery : IRequest<AdminClinicProviderDetailDto?>
{
    public Guid ClinicId { get; set; }
    public Guid ProviderId { get; set; }
}
