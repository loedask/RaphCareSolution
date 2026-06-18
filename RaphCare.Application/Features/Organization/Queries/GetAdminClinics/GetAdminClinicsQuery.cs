using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinics;

public sealed class GetAdminClinicsQuery : IRequest<PagedResult<ClinicListItemDto>>, IPlatformAdminRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
