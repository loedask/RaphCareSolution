using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.PatientInsurance.DTOs;

/// <summary>Patient-facing insurance profile with plan display name.</summary>
public class PatientInsuranceProfileDto : BaseDto
{
    public Guid InsurancePlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}
