using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Links a patient to an insurance plan or subscription.
/// </summary>
public class InsuranceProfile : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid InsurancePlanId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }

    public Patient Patient { get; set; } = null!;
}
