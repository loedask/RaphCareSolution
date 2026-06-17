namespace RaphCare.Client.Models.HealthRecords;

public class PagedHealthRecordsViewModel
{
    public IReadOnlyList<HealthRecordListItemViewModel> Items { get; set; } = Array.Empty<HealthRecordListItemViewModel>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
