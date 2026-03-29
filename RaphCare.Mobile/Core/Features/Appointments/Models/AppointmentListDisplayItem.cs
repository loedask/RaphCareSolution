namespace RaphCare.Mobile.Core.Features.Appointments.Models;

public sealed class AppointmentListDisplayItem
{
    public Guid Id { get; init; }
    public string PrimaryLine { get; init; } = string.Empty;
    public string SecondaryLine { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
