namespace RaphCare.Domain.Identity;

/// <summary>
/// Canonical identity role names seeded in <see cref="Role"/> and used for JWT authorization.
/// </summary>
public static class RaphCareRoles
{
    public const string Administrator = "Administrator";
    public const string Clinician = "Clinician";
    public const string Patient = "Patient";

    public static readonly string[] All = [Administrator, Clinician, Patient];

    public static readonly string[] Provider = [Administrator, Clinician];

    public static readonly string[] PatientPortal = [Patient, Administrator, Clinician];

    public static bool IsAdministrator(string roleName) =>
        string.Equals(roleName, Administrator, StringComparison.OrdinalIgnoreCase);

    public static bool IsClinician(string roleName) =>
        string.Equals(roleName, Clinician, StringComparison.OrdinalIgnoreCase);

    public static bool IsPatient(string roleName) =>
        string.Equals(roleName, Patient, StringComparison.OrdinalIgnoreCase);

    public static bool IsProviderRole(string roleName) =>
        IsAdministrator(roleName) || IsClinician(roleName);

    public static bool HasAdministratorRole(IEnumerable<string> roleNames) =>
        roleNames.Any(IsAdministrator);
}
