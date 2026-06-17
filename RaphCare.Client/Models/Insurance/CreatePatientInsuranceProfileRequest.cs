namespace RaphCare.Client.Models.Insurance;

public class CreatePatientInsuranceProfileRequest
{
    public Guid InsurancePlanId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
}
