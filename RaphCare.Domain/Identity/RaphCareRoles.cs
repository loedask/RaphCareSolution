namespace RaphCare.Domain.Identity;

/// <summary>
/// Canonical identity role names seeded in <see cref="Role"/> and used for JWT authorization.
/// </summary>
public static class RaphCareRoles
{
    public const string Administrator = "Administrator";
    public const string Clinician = "Clinician";
    public const string Doctor = "Doctor";
    public const string Pharmacist = "Pharmacist";
    public const string LabTechnician = "LabTechnician";
    public const string Patient = "Patient";

    public static readonly string[] All =
    [
        Administrator,
        Clinician,
        Doctor,
        Pharmacist,
        LabTechnician,
        Patient
    ];

    /// <summary>Staff job functions. A person holds at most one of these, plus optional Administrator.</summary>
    public static readonly string[] JobRoles =
    [
        Clinician,
        Doctor,
        Pharmacist,
        LabTechnician
    ];

    public static readonly string[] Provider =
    [
        Administrator,
        Clinician,
        Doctor,
        Pharmacist,
        LabTechnician
    ];

    public static readonly string[] PatientPortal = [Patient, Administrator, Clinician, Doctor, Pharmacist, LabTechnician];

    public static bool IsAdministrator(string roleName) =>
        string.Equals(roleName, Administrator, StringComparison.OrdinalIgnoreCase);

    public static bool IsClinician(string roleName) =>
        string.Equals(roleName, Clinician, StringComparison.OrdinalIgnoreCase);

    public static bool IsDoctor(string roleName) =>
        string.Equals(roleName, Doctor, StringComparison.OrdinalIgnoreCase);

    public static bool IsPharmacist(string roleName) =>
        string.Equals(roleName, Pharmacist, StringComparison.OrdinalIgnoreCase);

    public static bool IsLabTechnician(string roleName) =>
        string.Equals(roleName, LabTechnician, StringComparison.OrdinalIgnoreCase);

    public static bool IsPatient(string roleName) =>
        string.Equals(roleName, Patient, StringComparison.OrdinalIgnoreCase);

    public static bool IsProviderRole(string roleName) =>
        IsAdministrator(roleName)
        || IsClinician(roleName)
        || IsDoctor(roleName)
        || IsPharmacist(roleName)
        || IsLabTechnician(roleName);

    public static bool HasAdministratorRole(IEnumerable<string> roleNames) =>
        roleNames.Any(IsAdministrator);

    public static bool HasDoctorRole(IEnumerable<string> roleNames) =>
        roleNames.Any(IsDoctor);

    public static bool HasPharmacistRole(IEnumerable<string> roleNames) =>
        roleNames.Any(IsPharmacist);

    public static bool HasLabTechnicianRole(IEnumerable<string> roleNames) =>
        roleNames.Any(IsLabTechnician);

    public static bool HasClinicianRole(IEnumerable<string> roleNames) =>
        roleNames.Any(IsClinician);

    public static bool HasProviderJobRole(IEnumerable<string> roleNames) =>
        roleNames.Any(name => JobRoles.Any(job => string.Equals(job, name, StringComparison.OrdinalIgnoreCase)));

    public static bool CanDocumentVisits(IEnumerable<string> roleNames) =>
        HasAdministratorRole(roleNames) || HasDoctorRole(roleNames);

    public static bool CanDispensePrescriptions(IEnumerable<string> roleNames) =>
        HasAdministratorRole(roleNames) || HasPharmacistRole(roleNames) || HasClinicianRole(roleNames);

    public static bool CanCompleteLabs(IEnumerable<string> roleNames) =>
        HasAdministratorRole(roleNames) || HasLabTechnicianRole(roleNames) || HasClinicianRole(roleNames);

    /// <summary>Maps an Entra app-role value or staff form value to a seeded role name.</summary>
    public static string? MapExternalRole(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var value = raw.Trim();
        if (IsAdministrator(value))
            return Administrator;
        if (IsClinician(value))
            return Clinician;
        if (IsDoctor(value) || string.Equals(value, "Physician", StringComparison.OrdinalIgnoreCase))
            return Doctor;
        if (IsPharmacist(value))
            return Pharmacist;
        if (IsLabTechnician(value)
            || string.Equals(value, "LabTech", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "Lab Tech", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "Lab Technician", StringComparison.OrdinalIgnoreCase))
            return LabTechnician;
        if (IsPatient(value))
            return Patient;
        return null;
    }

    public static string NormalizeJobRole(string? raw)
    {
        var mapped = MapExternalRole(raw);
        if (mapped is not null && JobRoles.Contains(mapped, StringComparer.OrdinalIgnoreCase))
            return mapped;
        return Clinician;
    }
}
