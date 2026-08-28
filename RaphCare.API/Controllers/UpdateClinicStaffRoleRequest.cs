namespace RaphCare.API.Controllers;

public sealed class UpdateClinicStaffRoleRequest
{
    public bool IsAdministrator { get; set; }
    public string? JobRole { get; set; }
}
