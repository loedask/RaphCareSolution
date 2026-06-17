using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class TherapySession : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid TherapistId { get; set; }
    public Guid? TeleSessionId { get; set; }

    public DateTime SessionStart { get; set; }
    public DateTime? SessionEnd { get; set; }

    public string SessionType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Summary { get; set; }
    public bool IsConfidential { get; set; }

    public ICollection<TherapyNote> TherapyNotes { get; set; } = new List<TherapyNote>();
    public ICollection<CrisisFlag> CrisisFlags { get; set; } = new List<CrisisFlag>();
}

