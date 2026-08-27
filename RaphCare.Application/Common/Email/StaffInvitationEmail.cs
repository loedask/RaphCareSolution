namespace RaphCare.Application.Common.Email;

/// <summary>Clinic staff invitation messages for new and existing professional accounts.</summary>
public static class StaffInvitationEmail
{
    public static EmailContent ForExistingUser(string clinicName, string? displayName, string signInUrl)
    {
        var greetingName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        var subject = $"You are invited to {clinicName} on RaphCare";
        var hello = greetingName is null ? "Hello," : $"Hello {greetingName},";
        var plain =
            $"""
            {hello}

            You have been invited to join {clinicName} on the RaphCare clinic portal.

            Sign in with your professional account to open the hospital:
            {signInUrl}

            If you do not have an account yet, create a healthcare professional account first, then sign in.
            """;

        var html = RaphCareEmailLayout.Wrap(
            RaphCareEmailLayout.Heading($"You are invited to {clinicName}")
            + RaphCareEmailLayout.Greeting(greetingName)
            + RaphCareEmailLayout.Paragraph($"You have been invited to join {clinicName} on the RaphCare clinic portal.")
            + RaphCareEmailLayout.Paragraph("Sign in with your professional account to open the hospital.")
            + RaphCareEmailLayout.Button(signInUrl, "Sign in")
            + RaphCareEmailLayout.FallbackUrl(signInUrl)
            + RaphCareEmailLayout.Paragraph("If you do not have an account yet, create a healthcare professional account first, then sign in."),
            preheader: $"Join {clinicName} on RaphCare.");

        return new EmailContent(subject, plain, html);
    }

    public static EmailContent ForNewUser(string clinicName, string registerUrl)
    {
        var subject = $"Join {clinicName} on RaphCare";
        var plain =
            $"""
            Hello,

            You have been invited to join {clinicName} on the RaphCare clinic portal.

            Create your healthcare professional account using this email address:
            {registerUrl}

            After you register and sign in, you will be linked to the hospital automatically.
            """;

        var html = RaphCareEmailLayout.Wrap(
            RaphCareEmailLayout.Heading($"Join {clinicName} on RaphCare")
            + RaphCareEmailLayout.Greeting(null)
            + RaphCareEmailLayout.Paragraph($"You have been invited to join {clinicName} on the RaphCare clinic portal.")
            + RaphCareEmailLayout.Paragraph("Create your healthcare professional account with this email address. After you register and sign in, you will be linked to the hospital automatically.")
            + RaphCareEmailLayout.Button(registerUrl, "Create your account")
            + RaphCareEmailLayout.FallbackUrl(registerUrl),
            preheader: $"Create your RaphCare account to join {clinicName}.");

        return new EmailContent(subject, plain, html);
    }
}
