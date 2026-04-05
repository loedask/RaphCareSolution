using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// A person linked to the signed-in patient's account (e.g. spouse, child) for care coordination.
/// Stored under the owner's <see cref="Patient"/> record; optional <see cref="LinkedPatientId"/> when the member has their own chart.
/// </summary>
public class PatientFamilyMember : BaseEntity
{
    public Guid OwnerPatientId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Normalized label, e.g. Spouse, Child, Parent, Sibling, Other.</summary>
    public string Relationship { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    /// <summary>When the member is also a <see cref="Patient"/> in the system.</summary>
    public Guid? LinkedPatientId { get; set; }

    public bool IsActive { get; set; } = true;
}
