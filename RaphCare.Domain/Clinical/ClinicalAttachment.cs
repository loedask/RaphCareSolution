using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// File attachment associated with a visit.
/// </summary>
public class ClinicalAttachment : BaseEntity
{
    public Guid VisitId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public DateTime UploadedAt { get; set; }

    public Visit Visit { get; set; } = null!;
}
