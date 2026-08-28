using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicById;

public sealed class GetAdminClinicByIdQuery : IRequest<ClinicDetailDto?>
{
    public Guid ClinicId { get; set; }
}
