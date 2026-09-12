using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Appointments;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient appointment API (<c>api/patient/appointments</c>).</summary>
public interface IAppointmentService
{
    Task<Response<PagedAppointmentsViewModel>> GetMyAppointmentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Response<AppointmentViewModel?>> GetMyAppointmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<BookableProviderViewModel>>> GetBookableProvidersAsync(CancellationToken cancellationToken = default);
    Task<Response<Guid>> BookAsync(BookAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<Response<AppointmentConsentViewModel>> GetConsentAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<Response<AppointmentConsentViewModel>> AgreeConsentAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}
