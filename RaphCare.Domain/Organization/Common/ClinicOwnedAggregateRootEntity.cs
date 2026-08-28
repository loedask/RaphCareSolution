using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization.Common;

/// <summary>
/// Base class for clinic-owned entities that are also aggregate roots.
/// </summary>
public abstract class ClinicOwnedAggregateRootEntity : AggregateRoot
{
    [Required]
    public Guid ClinicId { get; set; }

    [ForeignKey(nameof(ClinicId))]
    public Clinic Clinic { get; set; } = null!;
}

