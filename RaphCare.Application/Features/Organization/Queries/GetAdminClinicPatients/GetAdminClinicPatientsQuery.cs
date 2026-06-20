using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatients;

public sealed class GetAdminClinicPatientsQuery : IRequest<PagedResult<AdminClinicPatientListItemDto>?>
{
    public Guid ClinicId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
}
