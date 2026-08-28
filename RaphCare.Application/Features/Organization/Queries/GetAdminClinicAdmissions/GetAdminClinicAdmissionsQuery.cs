using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissions;

public sealed class GetAdminClinicAdmissionsQuery : IRequest<PagedResult<AdminClinicAdmissionDto>?>
{
    public Guid ClinicId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
}
