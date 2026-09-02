using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicRosterBoard;

public sealed class GetAdminClinicRosterBoardQuery : IRequest<AdminClinicRosterBoardDto?>
{
    public Guid ClinicId { get; set; }
    public DateTime? DayUtc { get; set; }
}
