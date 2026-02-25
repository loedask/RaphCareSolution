namespace RaphCare.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string message, CancellationToken cancellationToken = default);
}

