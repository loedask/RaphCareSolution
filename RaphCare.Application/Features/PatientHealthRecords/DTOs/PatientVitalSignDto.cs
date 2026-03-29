namespace RaphCare.Application.Features.PatientHealthRecords.DTOs;

public class PatientVitalSignDto
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
}
