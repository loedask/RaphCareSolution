using MediatR;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;

namespace RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyCollectionOrders;

public sealed class GetMyCollectionOrdersQuery : IRequest<PatientCollectionOrdersDto>
{
}
