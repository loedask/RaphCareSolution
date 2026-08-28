namespace RaphCare.Client.Models.HealthRecords;

public class HealthRecordDetailViewModel
{
    public Guid Id { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public IReadOnlyList<VitalSignViewModel> VitalSigns { get; set; } = Array.Empty<VitalSignViewModel>();
    public IReadOnlyList<PatientCollectionPrescriptionViewModel> Prescriptions { get; set; } =
        Array.Empty<PatientCollectionPrescriptionViewModel>();
    public IReadOnlyList<PatientCollectionLabOrderViewModel> LabOrders { get; set; } =
        Array.Empty<PatientCollectionLabOrderViewModel>();
}
