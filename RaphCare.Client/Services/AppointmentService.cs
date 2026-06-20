using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class AppointmentService(HttpClient httpClient) : BaseHttpService(httpClient), IAppointmentService
{
    public Task<Response<PagedAppointmentsViewModel>> GetMyAppointmentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetAsync<PagedAppointmentsViewModel>($"api/patient/appointments?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    public Task<Response<AppointmentViewModel?>> GetMyAppointmentAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetAsync<AppointmentViewModel?>($"api/patient/appointments/{id}", cancellationToken);

    public async Task<Response<Guid>> BookAsync(BookAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedGuidApiResponse>("api/patient/appointments", request, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Booking failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }
}
