using RaphCare.Domain.Common;

namespace RaphCare.Domain.Billing;

/// <summary>Patient-facing care / app subscription tier (Vertical 9 — distinct from insurance <c>Subscription</c>).</summary>
public class PatientCarePlan : BaseEntity
{
    public Guid PatientId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanDisplayName { get; set; } = string.Empty;
    public int Tier { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? RenewsOn { get; set; }
    public string Status { get; set; } = string.Empty;
}
