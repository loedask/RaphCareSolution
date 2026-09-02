namespace RaphCare.API.Controllers;

public sealed class UpdateAdminClinicRequest
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool AllowAiDischargeDraft { get; set; } = true;
}
