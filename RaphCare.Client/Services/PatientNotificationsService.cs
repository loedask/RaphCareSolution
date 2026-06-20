using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Notifications;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientNotificationsService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientNotificationsService
{
    public async Task<Response<IReadOnlyList<PatientNotificationViewModel>>> GetMyNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<IReadOnlyList<NotificationDto>>("api/patient/notifications", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<PatientNotificationViewModel>>.Failure(result.ErrorMessage ?? "Could not load notifications.", result.StatusCode);

        var mapped = (result.Data ?? Array.Empty<NotificationDto>()).Select(Map).ToList();
        return Response<IReadOnlyList<PatientNotificationViewModel>>.Success(mapped);
    }

    public Task<Response<bool>> MarkReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        PutNoContentAsync($"api/patient/notifications/{id}/read", body: null, cancellationToken);

    public Task<Response<bool>> MarkAllReadAsync(CancellationToken cancellationToken = default) =>
        PutNoContentAsync("api/patient/notifications/read-all", body: null, cancellationToken);

    public Task<Response<bool>> RegisterPushDeviceAsync(RegisterPatientPushDeviceRequest request, CancellationToken cancellationToken = default) =>
        PutNoContentAsync("api/patient/notifications/push-device", request, cancellationToken);

    private static PatientNotificationViewModel Map(NotificationDto d) =>
        new()
        {
            Id = d.Id,
            Title = d.Title ?? string.Empty,
            Body = d.Body ?? string.Empty,
            Type = d.Type ?? string.Empty,
            IsRead = d.IsRead,
            CreatedAt = d.CreatedAt,
        };

    private sealed class NotificationDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
