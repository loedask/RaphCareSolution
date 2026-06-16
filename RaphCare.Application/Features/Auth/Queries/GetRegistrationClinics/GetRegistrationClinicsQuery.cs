using MediatR;

namespace RaphCare.Application.Features.Auth.Queries.GetRegistrationClinics;

public sealed class GetRegistrationClinicsQuery : IRequest<IReadOnlyList<RegistrationClinicDto>>
{
}
