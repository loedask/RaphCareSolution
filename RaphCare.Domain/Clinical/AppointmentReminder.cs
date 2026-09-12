using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Scheduled reminder for an appointment. Channel is typically InApp (patient app notice / push).
/// </summary>
public class AppointmentReminder : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public DateTime ReminderTime { get; set; }
    public string Channel { get; set; } = string.Empty; // InApp (v1); SMS / Email reserved
    public bool Sent { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
