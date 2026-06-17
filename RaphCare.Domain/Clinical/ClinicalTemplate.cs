using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Reusable documentation template for a clinic.
/// </summary>
public class ClinicalTemplate : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
    public string? Category { get; set; }
}
