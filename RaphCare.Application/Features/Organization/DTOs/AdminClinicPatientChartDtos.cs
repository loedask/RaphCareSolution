namespace RaphCare.Application.Features.Organization.DTOs;

/// <summary>Patient-reported medical summary plus structured allergy/medication rows for staff chart.</summary>
public sealed class AdminClinicPatientMedicalSummaryDto
{
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? Medications { get; set; }
    public string? PrimaryDoctor { get; set; }
    public IReadOnlyList<AdminClinicPatientAllergyDto> RecordedAllergies { get; set; } = Array.Empty<AdminClinicPatientAllergyDto>();
    public IReadOnlyList<AdminClinicPatientMedicationDto> RecordedMedications { get; set; } = Array.Empty<AdminClinicPatientMedicationDto>();
}

public sealed class AdminClinicPatientAllergyDto
{
    public string Substance { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public string? Severity { get; set; }
    public DateTime RecordedAt { get; set; }
}

public sealed class AdminClinicPatientMedicationDto
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class AdminClinicPatientEmergencyContactDto
{
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public sealed class AdminClinicPatientInsuranceDto
{
    public string PlanName { get; set; } = string.Empty;
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminClinicPatientInvoiceDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? VisitId { get; set; }
}

public sealed class AdminClinicPatientMoodLogDto
{
    public DateTime LoggedAt { get; set; }
    public int MoodScore { get; set; }
    public string? Notes { get; set; }
    public bool IsFlagged { get; set; }
}

public sealed class AdminClinicPatientCarePlanDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class AdminClinicVisitDiagnosisDto
{
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Severity { get; set; }
}

public sealed class AdminClinicVisitPrescriptionDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime IssuedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public string PickupCode { get; set; } = string.Empty;
    public DateTime? DispensedAt { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<AdminClinicVisitPrescriptionItemDto> Items { get; set; } = Array.Empty<AdminClinicVisitPrescriptionItemDto>();
}

public sealed class AdminClinicVisitPrescriptionItemDto
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }
}

public sealed class AdminClinicVisitNoteDto
{
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public sealed class AdminClinicVisitSoapNoteDto
{
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
}

public sealed class AdminClinicVisitLabResultDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string PickupCode { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string? ResultValue { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public DateTime? ReportedAt { get; set; }
}

/// <summary>Visit-scoped clinical documentation for a staff chart (read-only).</summary>
public sealed class AdminClinicVisitClinicalDocumentationDto
{
    public IReadOnlyList<AdminClinicVisitDiagnosisDto> Diagnoses { get; set; } = Array.Empty<AdminClinicVisitDiagnosisDto>();
    public IReadOnlyList<AdminClinicVisitPrescriptionDto> Prescriptions { get; set; } = Array.Empty<AdminClinicVisitPrescriptionDto>();
    public IReadOnlyList<AdminClinicVisitNoteDto> ClinicalNotes { get; set; } = Array.Empty<AdminClinicVisitNoteDto>();
    public IReadOnlyList<AdminClinicVisitSoapNoteDto> SoapNotes { get; set; } = Array.Empty<AdminClinicVisitSoapNoteDto>();
    public IReadOnlyList<AdminClinicVisitLabResultDto> LabResults { get; set; } = Array.Empty<AdminClinicVisitLabResultDto>();
}
