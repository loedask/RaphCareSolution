using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.EnsureCollectionDisplayToken;

public sealed class EnsureCollectionDisplayTokenHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Clinic> clinicRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<EnsureCollectionDisplayTokenCommand, CollectionDisplayLinkDto?>
{
    public async Task<CollectionDisplayLinkDto?> Handle(
        EnsureCollectionDisplayTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            return null;

        var token = clinic.CollectionDisplayToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await AllocateTokenAsync(clinicRepository, cancellationToken).ConfigureAwait(false);
            clinic.CollectionDisplayToken = token;
            await clinicRepository.UpdateAsync(clinic, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return new CollectionDisplayLinkDto { Token = token };
    }

    private static async Task<string> AllocateTokenAsync(
        IRepository<Clinic> clinicRepository,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 24; attempt++)
        {
            var token = ClinicCollectionDisplayToken.Generate();
            var existing = await clinicRepository.SearchAsync(
                q => q.Where(c => c.CollectionDisplayToken == token),
                1,
                1,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing.TotalCount == 0)
                return token;
        }

        throw new InvalidOperationException("Could not allocate a waiting-screen token.");
    }
}
