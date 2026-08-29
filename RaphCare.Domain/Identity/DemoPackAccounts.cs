namespace RaphCare.Domain.Identity;

/// <summary>Well-known Staging demo emails. Sign-in skips email verification for these addresses.</summary>
public static class DemoPackAccounts
{
    public const string AdminEmail = "demo.admin@raphcare.com";
    public const string DoctorEmail = "demo.doctor@raphcare.com";
    public const string PharmacistEmail = "demo.pharmacy@raphcare.com";
    public const string LabEmail = "demo.lab@raphcare.com";
    public const string PatientEmail = "demo.patient@raphcare.com";

    public static bool IsDemoEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalized = email.Trim().ToLowerInvariant();
        return normalized is AdminEmail or DoctorEmail or PharmacistEmail or LabEmail or PatientEmail;
    }
}
