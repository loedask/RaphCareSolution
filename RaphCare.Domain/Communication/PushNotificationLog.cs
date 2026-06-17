using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Communication;

public class PushNotificationLog : BaseEntity
{
    public Guid UserId { get; set; }

    public string DeviceToken { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime SentAt { get; set; }
}

