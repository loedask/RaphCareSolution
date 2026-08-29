using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Queries.GetRegistrationClinics;

public sealed class GetRegistrationClinicsQuery : IRequest<IReadOnlyList<RegistrationClinicDto>>, IAllowAnonymousRequest
{
    /// <summary>Optional name or reference-code filter (case-insensitive contains for names).</summary>
    public string? Search { get; init; }
}
