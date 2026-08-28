using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicCollectionOrders;

public sealed class GetAdminClinicCollectionOrdersQuery : IRequest<AdminClinicCollectionBoardDto?>
{
    public Guid ClinicId { get; set; }
    public string? Search { get; set; }
}
