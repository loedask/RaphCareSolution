namespace RaphCare.Client.Models.Appointments;

public class PagedAppointmentsViewModel
{
    public IReadOnlyList<AppointmentViewModel> Items { get; set; } = Array.Empty<AppointmentViewModel>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
