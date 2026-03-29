namespace RaphCare.Client.Models.Insurance;

public class PagedPatientInsuranceProfilesViewModel
{
    public IReadOnlyList<PatientInsuranceProfileViewModel> Items { get; set; } = Array.Empty<PatientInsuranceProfileViewModel>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
