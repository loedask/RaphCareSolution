using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.PatientHealthRecords.DTOs;

/// <summary>Patient-facing visit summary for the health records list.</summary>
public class PatientHealthRecordListItemDto : BaseDto
{
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
}
