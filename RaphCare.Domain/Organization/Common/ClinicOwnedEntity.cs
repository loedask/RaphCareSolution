using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization.Common;

/// <summary>
/// Base class for entities that are owned/scoped to a specific <see cref="Clinic"/>.
/// </summary>
public abstract class ClinicOwnedEntity : BaseEntity
{
    [Required]
    public Guid ClinicId { get; set; }

    [ForeignKey(nameof(ClinicId))]
    public Clinic Clinic { get; set; } = null!;
}

