using RaphCare.Domain.Common;

namespace RaphCare.Domain.AI;

public class SymptomReport : BaseEntity
{
    public Guid PatientId { get; set; }

    public string Description { get; set; } = null!;
    public DateTime ReportedAt { get; set; }
}

