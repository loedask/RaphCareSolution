using RaphCare.Domain.Common;

namespace RaphCare.Domain.Compliance;

/// <summary>
/// Mapping between FHIR resource types and internal domain entities.
/// </summary>
public class FHIRResourceMapping : BaseEntity
{
    public string ResourceType { get; set; } = string.Empty;
    public string InternalEntity { get; set; } = string.Empty;
    public string MappingJson { get; set; } = string.Empty;
}
