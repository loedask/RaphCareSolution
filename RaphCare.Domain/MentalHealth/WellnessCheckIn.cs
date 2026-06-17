using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class WellnessCheckIn : BaseEntity
{
    public Guid PatientId { get; set; }

    public DateTime CheckedInAt { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public bool AIReviewed { get; set; }
}

