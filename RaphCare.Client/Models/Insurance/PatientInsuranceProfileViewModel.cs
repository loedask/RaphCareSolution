namespace RaphCare.Client.Models.Insurance;

public class PatientInsuranceProfileViewModel
{
    public Guid Id { get; set; }
    public Guid InsurancePlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}
