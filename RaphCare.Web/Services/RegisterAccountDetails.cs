using System.Net.Mail;

namespace RaphCare.Web.Services;

/// <summary>Client-side checks that match email registration rules, so we do not send a code first.</summary>
public static class RegisterAccountDetails
{
    public const int MinPasswordLength = 8;
    public const int MaxEmailLength = 256;

    public static string? Validate(string firstName, string lastName, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return "Common_PleaseEnterFirstName";
        if (string.IsNullOrWhiteSpace(lastName))
            return "Common_PleaseEnterLastName";

        var trimmedEmail = email.Trim();
        if (string.IsNullOrWhiteSpace(trimmedEmail))
            return "Common_PleaseEnterEmail";
        if (!IsValidEmail(trimmedEmail))
            return "Common_PleaseEnterValidEmail";

        if (string.IsNullOrWhiteSpace(password))
            return "Common_PleaseEnterPassword";
        if (password.Length < MinPasswordLength)
            return "Common_PasswordMinLength";
        if (string.IsNullOrWhiteSpace(confirmPassword))
            return "Common_PleaseConfirmPassword";
        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            return "Common_PasswordsDoNotMatch";

        return null;
    }

    public static bool LooksLikeDetailsError(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        return message.Contains("Password", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Email", StringComparison.OrdinalIgnoreCase)
            || message.Contains("FirstName", StringComparison.OrdinalIgnoreCase)
            || message.Contains("LastName", StringComparison.OrdinalIgnoreCase)
            || message.Contains("First Name", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Last Name", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidEmail(string email)
    {
        if (email.Length is 0 or > MaxEmailLength)
            return false;

        try
        {
            var parsed = new MailAddress(email);
            return string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
