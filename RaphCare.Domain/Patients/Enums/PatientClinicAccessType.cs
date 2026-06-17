namespace RaphCare.Domain.Patients.Enums;

/// <summary>
/// How a patient gained access to a specific clinic.
/// </summary>
public enum PatientClinicAccessType
{
    /// <summary>
    /// Access derived from encounters (visit, appointment, or tele-session).
    /// </summary>
    EncounterBased = 1,

    /// <summary>
    /// Access granted because the patient is registered/associated with the clinic.
    /// </summary>
    Registered = 2,

    /// <summary>
    /// Access granted because an insurance profile is linked to the clinic.
    /// </summary>
    InsuranceLinked = 3,

    /// <summary>
    /// Access granted manually (e.g. admin/provider override).
    /// </summary>
    ManualGrant = 4
}

