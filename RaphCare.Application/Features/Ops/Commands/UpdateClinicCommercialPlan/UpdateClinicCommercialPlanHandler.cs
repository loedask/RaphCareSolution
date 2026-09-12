using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Ops.Queries.GetClinicCommercialPlan;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Ops.Commands.UpdateClinicCommercialPlan;

public sealed class UpdateClinicCommercialPlanHandler(
    IRepository<Clinic> clinicRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<UpdateClinicCommercialPlanCommand, ClinicCommercialPlanDto?>
{
    public async Task<ClinicCommercialPlanDto?> Handle(
        UpdateClinicCommercialPlanCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsPlatformAdministratorAsync(
                currentUserService, roleAssignmentService, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only platform Ops can assign commercial plans.");

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            return null;

        var plan = ClinicCommercialPlan.Normalize(request.CommercialPlan);
        clinic.CommercialPlan = plan;
        await clinicRepository.UpdateAsync(clinic, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
