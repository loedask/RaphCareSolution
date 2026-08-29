using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Auth.Queries.GetRegistrationClinics;

public sealed class ResolveRegistrationClinicByReferenceHandler(IRepository<Clinic> clinicRepository)
    : IRequestHandler<ResolveRegistrationClinicByReferenceQuery, RegistrationClinicDto?>
{
    public async Task<RegistrationClinicDto?> Handle(
        ResolveRegistrationClinicByReferenceQuery request,
        CancellationToken cancellationToken)
    {
        if (!ClinicReferenceCode.TryNormalize(request.ReferenceCode, out var code))
            return null;

        var paged = await clinicRepository.SearchAsync(
            queryShaper: q => q.Where(c =>
                c.IsActive && !c.IsDeleted && c.ReferenceCode == code),
            pageNumber: 1,
            pageSize: 1,
            applyDefaultIdOrdering: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var clinic = paged.Items.Count > 0 ? paged.Items[0] : null;
        if (clinic is null)
            return null;

        return new RegistrationClinicDto
        {
            Id = clinic.Id,
            Name = clinic.Name,
            ReferenceCode = clinic.ReferenceCode
        };
    }
}