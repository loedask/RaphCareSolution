using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Booking before clinical interaction. Aggregate root for appointment consistency.
/// </summary>
public class Appointment : AggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid? FacilityId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty; // Telemedicine / InPerson
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool IsCancelled { get; set; }

    public ICollection<AppointmentReminder> AppointmentReminders { get; set; } = new List<AppointmentReminder>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
