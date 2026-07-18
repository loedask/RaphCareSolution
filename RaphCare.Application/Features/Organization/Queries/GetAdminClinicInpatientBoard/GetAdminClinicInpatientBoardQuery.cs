using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicInpatientBoard;

public sealed class GetAdminClinicInpatientBoardQuery : IRequest<AdminClinicInpatientBoardDto?>
{
    public Guid ClinicId { get; set; }
}
