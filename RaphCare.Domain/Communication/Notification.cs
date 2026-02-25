using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Communication;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsRead { get; set; }
}

