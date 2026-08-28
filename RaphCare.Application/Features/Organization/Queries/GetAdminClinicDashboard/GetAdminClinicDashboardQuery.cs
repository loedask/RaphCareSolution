using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicDashboard;

public sealed class GetAdminClinicDashboardQuery : IRequest<AdminClinicDashboardDto?>
{
    public Guid ClinicId { get; set; }
}
