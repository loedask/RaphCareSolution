using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Reminder sent for an appointment (SMS / Email / Push).
/// </summary>
public class AppointmentReminder : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public DateTime ReminderTime { get; set; }
    public string Channel { get; set; } = string.Empty; // SMS / Email / Push
    public bool Sent { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
