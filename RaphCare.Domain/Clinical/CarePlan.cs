using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Longitudinal care management plan. Aggregate root.
/// </summary>
public class CarePlan : AggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public ICollection<CarePlanTask> CarePlanTasks { get; set; } = new List<CarePlanTask>();
}
