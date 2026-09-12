using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Ops.Queries.GetClinicCommercialPlan;

public sealed class GetClinicCommercialPlanHandler(
    IRepository<Clinic> clinicRepository,
    ICurrentUserService currentUserService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<GetClinicCommercialPlanQuery, ClinicCommercialPlanDto?>
{
    public async Task<ClinicCommercialPlanDto?> Handle(
        GetClinicCommercialPlanQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsPlatformAdministratorAsync(
                currentUserService, roleAssignmentService, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only platform Ops can view commercial plans.");

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            return null;

        var plan = ClinicCommercialPlan.Normalize(clinic.CommercialPlan);
        return new ClinicCommercialPlanDto
        {
            ClinicId = clinic.Id,
            CommercialPlan = plan,
            HasInpatient = ClinicCommercialPlanFeatures.HasInpatient(plan),
            HasCollection = ClinicCommercialPlanFeatures.HasCollection(plan),
            HasCasualty = ClinicCommercialPlanFeatures.HasCasualty(plan),
            HasTheatre = ClinicCommercialPlanFeatures.HasTheatre(plan),
            HasConsultWaiting = ClinicCommercialPlanFeatures.HasConsultWaiting(plan)
        };
    }
}
