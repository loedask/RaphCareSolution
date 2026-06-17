using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Communication;

public class SMSLog : BaseEntity
{
    public string PhoneNumber { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public bool Successful { get; set; }
}

