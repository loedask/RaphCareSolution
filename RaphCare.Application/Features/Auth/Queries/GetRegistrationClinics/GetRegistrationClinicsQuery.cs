using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Queries.GetRegistrationClinics;

public sealed class GetRegistrationClinicsQuery : IRequest<IReadOnlyList<RegistrationClinicDto>>, IAllowAnonymousRequest
{
}
