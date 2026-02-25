using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Result for a lab request.
/// </summary>
public class LabResult : BaseEntity
{
    public Guid LabRequestId { get; set; }
    public string ResultValue { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public DateTime ReportedAt { get; set; }

    public LabRequest LabRequest { get; set; } = null!;
}
