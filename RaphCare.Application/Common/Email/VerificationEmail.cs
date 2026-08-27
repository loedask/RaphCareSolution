namespace RaphCare.Application.Common.Email;

/// <summary>Subject and bodies for the 6-digit verification email.</summary>
public static class VerificationEmail
{
    public const string Subject = "Your RaphCare verification code";

    public static EmailContent Create(string code)
    {
        var plain =
            $"""
            Your RaphCare verification code is {code}.

            Enter it on the website or in the app to continue. It expires in 10 minutes.

            If you did not request this, you can ignore this email.
            """;

        var html = RaphCareEmailLayout.Wrap(
            RaphCareEmailLayout.Heading("Confirm it is you")
            + RaphCareEmailLayout.Paragraph("Enter this code to continue. It expires in 10 minutes.")
            + RaphCareEmailLayout.CodeBox(code)
            + RaphCareEmailLayout.Paragraph("If you did not request this, you can ignore this email."),
            preheader: $"Your verification code is {code}. It expires in 10 minutes.");

        return new EmailContent(Subject, plain, html);
    }
}
