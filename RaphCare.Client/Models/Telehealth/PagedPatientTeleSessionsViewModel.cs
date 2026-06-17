namespace RaphCare.Client.Models.Telehealth;

public class PagedPatientTeleSessionsViewModel
{
    public IReadOnlyList<PatientTeleSessionListItemViewModel> Items { get; set; } = Array.Empty<PatientTeleSessionListItemViewModel>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
