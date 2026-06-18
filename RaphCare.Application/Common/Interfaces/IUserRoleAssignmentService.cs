namespace RaphCare.Application.Common.Interfaces;

public interface IUserRoleAssignmentService
{
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AssignRoleIfMissingAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
}
