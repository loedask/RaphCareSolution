using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Auth.Queries.GetRegistrationClinics;

public sealed class GetRegistrationClinicsHandler(IRepository<Clinic> clinicRepository)
    : IRequestHandler<GetRegistrationClinicsQuery, IReadOnlyList<RegistrationClinicDto>>
{
    public async Task<IReadOnlyList<RegistrationClinicDto>> Handle(
        GetRegistrationClinicsQuery request,
        CancellationToken cancellationToken)
    {
        var paged = await clinicRepository.SearchAsync(
            queryShaper: q => q
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Name),
            pageNumber: 1,
            pageSize: 200,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return paged.Items
            .Select(c => new RegistrationClinicDto { Id = c.Id, Name = c.Name })
            .ToList();
    }
}
