namespace RaphCare.Client.Models.Ops;

public sealed class ClinicCommercialPlanInfo
{
    public Guid ClinicId { get; set; }
    public string CommercialPlan { get; set; } = string.Empty;
    public bool HasInpatient { get; set; }
    public bool HasCollection { get; set; }
    public bool HasCasualty { get; set; }
    public bool HasTheatre { get; set; }
    public bool HasConsultWaiting { get; set; }
}
