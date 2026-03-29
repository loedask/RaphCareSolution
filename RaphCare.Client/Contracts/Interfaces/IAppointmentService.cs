using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Appointments;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient appointment API (<c>api/patient/appointments</c>). Uses HttpClient until NSwag includes these routes.</summary>
public interface IAppointmentService
{
    Task<Response<PagedAppointmentsViewModel>> GetMyAppointmentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Response<AppointmentViewModel?>> GetMyAppointmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Response<Guid>> BookAsync(BookAppointmentRequest request, CancellationToken cancellationToken = default);
}
