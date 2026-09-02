using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.EnsureCasualtyDisplayToken;

public sealed class EnsureCasualtyDisplayTokenHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Clinic> clinicRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<EnsureCasualtyDisplayTokenCommand, CasualtyDisplayLinkDto?>
{
    public async Task<CasualtyDisplayLinkDto?> Handle(
        EnsureCasualtyDisplayTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            return null;

        var token = clinic.CasualtyDisplayToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await AllocateTokenAsync(clinicRepository, cancellationToken).ConfigureAwait(false);
            clinic.CasualtyDisplayToken = token;
            await clinicRepository.UpdateAsync(clinic, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return new CasualtyDisplayLinkDto { Token = token };
    }

    private static async Task<string> AllocateTokenAsync(
        IRepository<Clinic> clinicRepository,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 24; attempt++)
        {
            var token = ClinicCollectionDisplayToken.Generate();
            var existing = await clinicRepository.SearchAsync(
                q => q.Where(c => c.CasualtyDisplayToken == token),
                1,
                1,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing.TotalCount == 0)
                return token;
        }

        throw new InvalidOperationException("Could not allocate a casualty waiting-screen token.");
    }
}
