using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicVisitById;

public sealed class GetAdminClinicVisitByIdQuery : IRequest<AdminClinicVisitDetailDto?>
{
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
}
