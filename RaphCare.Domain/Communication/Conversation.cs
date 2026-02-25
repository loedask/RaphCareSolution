using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Communication;

public class Conversation : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }

    public string Subject { get; set; } = null!;
    public bool IsClosed { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

