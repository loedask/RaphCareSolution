using RaphCare.Domain.Common;

namespace RaphCare.Domain.Insurance;

/// <summary>
/// Insurance plan (medical aid) that can be linked to patient profiles.
/// </summary>
public class InsurancePlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
