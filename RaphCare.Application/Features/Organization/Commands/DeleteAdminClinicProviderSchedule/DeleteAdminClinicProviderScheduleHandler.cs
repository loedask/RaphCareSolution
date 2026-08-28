using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicProviderSchedule;

public sealed class DeleteAdminClinicProviderScheduleHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Provider> providerRepository,
    IRepository<ProviderSchedule> scheduleRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAdminClinicProviderScheduleCommand, bool>
{
    public async Task<bool> Handle(DeleteAdminClinicProviderScheduleCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            return false;

        var provider = await providerRepository.GetByIdAsync(request.ProviderId, cancellationToken).ConfigureAwait(false);
        if (provider is null || provider.ClinicId != request.ClinicId || provider.IsDeleted)
            return false;

        var schedule = await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken).ConfigureAwait(false);
        if (schedule is null || schedule.ProviderId != request.ProviderId)
            return false;

        await scheduleRepository.DeleteAsync(schedule, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
