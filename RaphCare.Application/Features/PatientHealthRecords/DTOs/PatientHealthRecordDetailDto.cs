namespace RaphCare.Application.Features.PatientHealthRecords.DTOs;

/// <summary>Patient-facing visit detail including vitals captured during the visit.</summary>
public class PatientHealthRecordDetailDto
{
    public Guid Id { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public IReadOnlyList<PatientVitalSignDto> VitalSigns { get; init; } = Array.Empty<PatientVitalSignDto>();
    public IReadOnlyList<PatientCollectionPrescriptionDto> Prescriptions { get; init; } =
        Array.Empty<PatientCollectionPrescriptionDto>();
    public IReadOnlyList<PatientCollectionLabOrderDto> LabOrders { get; init; } =
        Array.Empty<PatientCollectionLabOrderDto>();
}
