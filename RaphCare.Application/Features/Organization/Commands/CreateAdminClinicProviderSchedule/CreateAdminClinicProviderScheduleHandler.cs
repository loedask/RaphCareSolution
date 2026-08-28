using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicProviderSchedule;

public sealed class CreateAdminClinicProviderScheduleHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Provider> providerRepository,
    IRepository<ProviderSchedule> scheduleRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicProviderScheduleCommand, AdminClinicProviderScheduleDto?>
{
    public async Task<AdminClinicProviderScheduleDto?> Handle(
        CreateAdminClinicProviderScheduleCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can manage schedules.");

        var provider = await providerRepository.GetByIdAsync(request.ProviderId, cancellationToken).ConfigureAwait(false);
        if (provider is null || provider.ClinicId != request.ClinicId || provider.IsDeleted)
            return null;

        var schedule = new ProviderSchedule
        {
            ProviderId = provider.Id,
            Day = request.Day,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsRecurring = true
        };

        await scheduleRepository.AddAsync(schedule, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicProviderScheduleDto
        {
            Id = schedule.Id,
            Day = schedule.Day,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            IsRecurring = schedule.IsRecurring
        };
    }
}
