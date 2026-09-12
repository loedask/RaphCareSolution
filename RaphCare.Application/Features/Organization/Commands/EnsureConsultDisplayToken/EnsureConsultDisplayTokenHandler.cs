using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.EnsureConsultDisplayToken;

public sealed class EnsureConsultDisplayTokenHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Clinic> clinicRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<EnsureConsultDisplayTokenCommand, ConsultDisplayLinkDto?>
{
    public async Task<ConsultDisplayLinkDto?> Handle(
        EnsureConsultDisplayTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            return null;

        var token = clinic.ConsultDisplayToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await AllocateTokenAsync(clinicRepository, cancellationToken).ConfigureAwait(false);
            clinic.ConsultDisplayToken = token;
            await clinicRepository.UpdateAsync(clinic, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return new ConsultDisplayLinkDto { Token = token };
    }

    private static async Task<string> AllocateTokenAsync(
        IRepository<Clinic> clinicRepository,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 24; attempt++)
        {
            var token = ClinicCollectionDisplayToken.Generate();
            var existing = await clinicRepository.SearchAsync(
                q => q.Where(c => c.ConsultDisplayToken == token),
                1,
                1,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing.TotalCount == 0)
                return token;
        }

        throw new InvalidOperationException("Could not allocate a consult waiting-screen token.");
    }
}
