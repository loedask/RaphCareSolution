using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Follow-up instruction given at the end of a visit.
/// </summary>
public class FollowUpInstruction : BaseEntity
{
    public Guid VisitId { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool Completed { get; set; }

    public Visit Visit { get; set; } = null!;
}
