using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Insurance.DTOs;

public class InsuranceProfileDto : BaseDto
{
    public Guid PatientId { get; set; }
    public Guid InsurancePlanId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}

