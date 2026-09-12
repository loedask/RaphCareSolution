namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicConsentTemplateDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
