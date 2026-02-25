using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Provider availability slot for booking.
/// </summary>
public class AppointmentSlot : BaseEntity
{
    public Guid ProviderId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
}
