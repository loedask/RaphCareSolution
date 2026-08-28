namespace RaphCare.API.Controllers;

public sealed class StartAdminClinicVisitRequest
{
    public string? Summary { get; set; }
}

public sealed class CompleteAdminClinicVisitRequest
{
    public string? Summary { get; set; }
}

public sealed class CreateAdminClinicVisitVitalRequest
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime? RecordedAt { get; set; }
}

public sealed class UpsertAdminClinicVisitSoapNoteRequest
{
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
}

public sealed class CreateAdminClinicVisitNoteRequest
{
    public string Notes { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public sealed class CreateAdminClinicVisitPrescriptionRequest
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }
    public string? Notes { get; set; }
}

public sealed class CreateAdminClinicVisitLabResultRequest
{
    public string TestName { get; set; } = string.Empty;
    public string ResultValue { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
}
