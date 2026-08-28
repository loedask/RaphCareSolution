namespace RaphCare.API.Controllers;

public sealed class GrantAdminClinicPatientAccessRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
