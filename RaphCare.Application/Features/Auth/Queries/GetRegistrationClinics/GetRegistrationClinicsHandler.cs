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
        var search = request.Search?.Trim();
        var hasExactReference = ClinicReferenceCode.TryNormalize(search, out var referenceCode);

        var paged = await clinicRepository.SearchAsync(
            queryShaper: q =>
            {
                var filtered = q.Where(c => c.IsActive && !c.IsDeleted);

                if (hasExactReference)
                {
                    filtered = filtered.Where(c => c.ReferenceCode == referenceCode);
                }
                else if (!string.IsNullOrEmpty(search))
                {
                    filtered = filtered.Where(c =>
                        c.Name.Contains(search)
                        || c.ReferenceCode.Contains(search));
                }

                return filtered.OrderBy(c => c.Name);
            },
            pageNumber: 1,
            pageSize: 200,
            applyDefaultIdOrdering: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return paged.Items
            .Select(c => new RegistrationClinicDto
            {
                Id = c.Id,
                Name = c.Name,
                ReferenceCode = c.ReferenceCode
            })
            .ToList();
    }
}
