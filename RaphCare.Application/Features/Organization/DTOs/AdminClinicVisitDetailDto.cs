namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicVisitDetailDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal? InvoiceAmount { get; set; }
    public string? InvoiceStatus { get; set; }
    public string? InvoiceCurrency { get; set; }
    /// <summary>True when the patient has agreed to the practice pre-visit consent for this appointment.</summary>
    public bool ConsentSigned { get; set; }
    public DateTime? ConsentSignedAt { get; set; }
    public IReadOnlyList<AdminClinicVisitVitalDto> Vitals { get; set; } = Array.Empty<AdminClinicVisitVitalDto>();
    public IReadOnlyList<AdminClinicVisitDiagnosisDto> Diagnoses { get; set; } = Array.Empty<AdminClinicVisitDiagnosisDto>();
    public IReadOnlyList<AdminClinicVisitPrescriptionDto> Prescriptions { get; set; } = Array.Empty<AdminClinicVisitPrescriptionDto>();
    public IReadOnlyList<AdminClinicVisitNoteDto> ClinicalNotes { get; set; } = Array.Empty<AdminClinicVisitNoteDto>();
    public IReadOnlyList<AdminClinicVisitSoapNoteDto> SoapNotes { get; set; } = Array.Empty<AdminClinicVisitSoapNoteDto>();
    public IReadOnlyList<AdminClinicVisitLabResultDto> LabResults { get; set; } = Array.Empty<AdminClinicVisitLabResultDto>();
}

public sealed class AdminClinicVisitVitalDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
}

public sealed class AdminClinicDeviceListItemDto
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? BluetoothMacAddress { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsAssigned { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedPatientId { get; set; }
    public string? AssignedPatientName { get; set; }
}
