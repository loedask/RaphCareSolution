namespace RaphCare.Application.Features.PatientHealthRecords.DTOs;

public sealed class PatientCollectionOrdersDto
{
    public IReadOnlyList<PatientCollectionPrescriptionDto> Prescriptions { get; set; } =
        Array.Empty<PatientCollectionPrescriptionDto>();

    public IReadOnlyList<PatientCollectionLabOrderDto> LabOrders { get; set; } =
        Array.Empty<PatientCollectionLabOrderDto>();
}

public sealed class PatientCollectionPrescriptionDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string PickupCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CalledAt { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<PatientCollectionPrescriptionItemDto> Items { get; set; } =
        Array.Empty<PatientCollectionPrescriptionItemDto>();
}

public sealed class PatientCollectionPrescriptionItemDto
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }
}

public sealed class PatientCollectionLabOrderDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string PickupCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public string? ResultValue { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public DateTime? ReportedAt { get; set; }
}
