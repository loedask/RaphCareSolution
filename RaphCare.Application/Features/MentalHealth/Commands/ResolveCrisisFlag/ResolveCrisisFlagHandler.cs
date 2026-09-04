using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth.Commands.ResolveCrisisFlag;

public sealed class ResolveCrisisFlagHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<TherapySession> sessionRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResolveCrisisFlagCommand, CrisisFlagDto?>
{
    public async Task<CrisisFlagDto?> Handle(
        ResolveCrisisFlagCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can resolve a crisis flag.",
                cancellationToken)
            .ConfigureAwait(false);

        var page = await sessionRepository.SearchAsync(
                q => q
                    .Where(s => s.Id == request.SessionId && s.ClinicId == request.ClinicId)
                    .Include(s => s.CrisisFlags),
                1,
                1,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        var session = page.Items.Count > 0 ? page.Items[0] : null;
        var flag = session?.CrisisFlags.FirstOrDefault(c => c.Id == request.CrisisFlagId);
        if (flag is null)
            return null;

        flag.IsResolved = true;
        flag.ResolvedAt = clock.UtcNow;
        await sessionRepository.UpdateAsync(session!, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CrisisFlagDto
        {
            Id = flag.Id,
            RiskLevel = flag.RiskLevel,
            Description = flag.Description,
            FlaggedAt = flag.FlaggedAt,
            IsResolved = flag.IsResolved,
            ResolvedAt = flag.ResolvedAt
        };
    }
}
