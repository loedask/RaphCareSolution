namespace RaphCare.Client.Models.HealthRecords;

public sealed class PatientCollectionOrdersViewModel
{
    public IReadOnlyList<PatientCollectionPrescriptionViewModel> Prescriptions { get; set; } =
        Array.Empty<PatientCollectionPrescriptionViewModel>();

    public IReadOnlyList<PatientCollectionLabOrderViewModel> LabOrders { get; set; } =
        Array.Empty<PatientCollectionLabOrderViewModel>();
}

public sealed class PatientCollectionPrescriptionViewModel
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
    public IReadOnlyList<PatientCollectionPrescriptionItemViewModel> Items { get; set; } =
        Array.Empty<PatientCollectionPrescriptionItemViewModel>();
}

public sealed class PatientCollectionPrescriptionItemViewModel
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }
}

public sealed class PatientCollectionLabOrderViewModel
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
