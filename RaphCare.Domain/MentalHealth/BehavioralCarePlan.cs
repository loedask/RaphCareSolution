using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class BehavioralCarePlan : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public ICollection<TherapyGoal> Goals { get; set; } = new List<TherapyGoal>();
}

