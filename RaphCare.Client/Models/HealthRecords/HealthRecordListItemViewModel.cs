namespace RaphCare.Client.Models.HealthRecords;

public class HealthRecordListItemViewModel
{
    public Guid Id { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string RecordKind { get; set; } = "Visit";
}
