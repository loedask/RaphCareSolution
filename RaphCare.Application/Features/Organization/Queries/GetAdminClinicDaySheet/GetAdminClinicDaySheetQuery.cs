using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicDaySheet;

public sealed class GetAdminClinicDaySheetQuery : IRequest<IReadOnlyList<AdminClinicDaySheetItemDto>?>
{
    public Guid ClinicId { get; set; }
}
