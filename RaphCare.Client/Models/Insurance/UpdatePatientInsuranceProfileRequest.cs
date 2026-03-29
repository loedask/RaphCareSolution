namespace RaphCare.Client.Models.Insurance;

public class UpdatePatientInsuranceProfileRequest
{
    public Guid Id { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}
