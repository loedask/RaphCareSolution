using MediatR;
using RaphCare.Application.Features.Appointments.DTOs;

namespace RaphCare.Application.Features.Appointments.Queries.GetBookableProviders;

public sealed class GetBookableProvidersQuery : IRequest<IReadOnlyList<BookableProviderDto>>
{
}
