using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicAppointments;

public sealed class GetAdminClinicAppointmentsQuery : IRequest<PagedResult<AdminClinicAppointmentListItemDto>?>
{
    public Guid ClinicId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? Status { get; set; }
}
