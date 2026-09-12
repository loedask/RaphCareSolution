using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Practice pre-visit consent text for a clinic. At most one template should be active per clinic.
/// </summary>
public class ClinicConsentTemplate : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
