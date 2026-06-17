using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Communication;

public class MessageAttachment : BaseEntity
{
    public Guid MessageId { get; set; }

    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;

    public Message Message { get; set; } = null!;
}

