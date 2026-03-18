namespace RaphCare.Domain.Patients.Enums;

/// <summary>
/// Patient identity timeline event types for audit/forensics and MPI reconciliation support.
/// </summary>
public enum PatientIdentityEventType
{
    PatientCreated = 1,
    IdentityLinked = 2,
    IdentityUnlinked = 3,
    PatientMerged = 4,
    PatientUpdated = 5
}

