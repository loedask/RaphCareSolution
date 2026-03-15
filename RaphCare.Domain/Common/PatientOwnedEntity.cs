using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RaphCare.Domain.Patients;

namespace RaphCare.Domain.Common;

/// <summary>
/// Base class for entities that are owned or associated with a specific Patient.
/// Inherits common properties from BaseEntity and enforces a required foreign key relationship with Patient.
/// </summary>
public abstract class PatientOwnedEntity : BaseEntity
{
    #region Patient Foreign Key

    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient? Patient { get; set; }

    #endregion
}
