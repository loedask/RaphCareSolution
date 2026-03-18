using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Clinical specialty (e.g. psychiatry, psychology, counselling).
/// </summary>
public class Specialty : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Inverse navigations to support many-to-many relationships.
    public ICollection<Provider> Providers { get; set; } = new List<Provider>();
    public ICollection<Therapist> Therapists { get; set; } = new List<Therapist>();
}
