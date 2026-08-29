namespace RaphCare.Application.Common.Email;

/// <summary>Subject and bodies for the password-reset verification email.</summary>
public static class PasswordResetEmail
{
    public const string Subject = "Reset your RaphCare password";

    public static EmailContent Create(string code)
    {
        var plain =
            $"""
            Your RaphCare password reset code is {code}.

            Enter it in the app with your new password. It expires in 10 minutes.

            If you did not ask to reset your password, you can ignore this email.
            """;

        var html = RaphCareEmailLayout.Wrap(
            RaphCareEmailLayout.Heading("Reset your password")
            + RaphCareEmailLayout.Paragraph("Enter this code in the app with your new password. It expires in 10 minutes.")
            + RaphCareEmailLayout.CodeBox(code)
            + RaphCareEmailLayout.Paragraph("If you did not ask to reset your password, you can ignore this email."),
            preheader: $"Your password reset code is {code}. It expires in 10 minutes.");

        return new EmailContent(Subject, plain, html);
    }
}
