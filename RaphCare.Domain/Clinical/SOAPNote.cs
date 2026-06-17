using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// SOAP (Subjective, Objective, Assessment, Plan) note for a visit (1-1).
/// </summary>
public class SOAPNote : BaseEntity
{
    public Guid VisitId { get; set; }
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }

    public Visit Visit { get; set; } = null!;
}
