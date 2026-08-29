using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Queries.GetRegistrationClinics;

public sealed class ResolveRegistrationClinicByReferenceQuery
    : IRequest<RegistrationClinicDto?>, IAllowAnonymousRequest
{
    public required string ReferenceCode { get; init; }
}
