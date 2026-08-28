namespace RaphCare.Application.Common.Interfaces;

public interface IClinicStaffInvitationService
{
    Task SendInvitationAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken = default);
}
