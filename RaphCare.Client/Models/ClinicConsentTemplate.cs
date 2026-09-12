namespace RaphCare.Client.Models;

public sealed class ClinicConsentTemplate
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class UpsertClinicConsentTemplateRequest
{
    public Guid? TemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
