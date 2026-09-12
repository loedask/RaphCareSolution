namespace RaphCare.API.Controllers;

public sealed class UpsertAdminClinicConsentTemplateRequest
{
    public Guid? TemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
