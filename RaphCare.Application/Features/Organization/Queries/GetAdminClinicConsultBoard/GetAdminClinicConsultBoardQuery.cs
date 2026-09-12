using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicConsultBoard;

public sealed class GetAdminClinicConsultBoardQuery : IRequest<AdminClinicConsultBoardDto?>
{
    public Guid ClinicId { get; set; }
}
