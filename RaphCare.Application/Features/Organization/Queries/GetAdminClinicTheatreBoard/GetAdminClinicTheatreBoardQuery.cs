using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicTheatreBoard;

public sealed class GetAdminClinicTheatreBoardQuery : IRequest<AdminClinicTheatreBoardDto?>
{
    public Guid ClinicId { get; set; }
    public DateTime? DayUtc { get; set; }
}
