using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Vital sign recorded during a visit (BP, HR, Temp, etc.).
/// </summary>
public class VitalSignRecord : BaseEntity
{
    public Guid VisitId { get; set; }
    public string Type { get; set; } = string.Empty; // BP, HR, Temp
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }

    public Visit Visit { get; set; } = null!;
}
