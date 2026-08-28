namespace RaphCare.Application.Common.Email;

/// <summary>Inbound patient support ticket forwarded to the support mailbox.</summary>
public static class SupportTicketEmail
{
    public static EmailContent Create(
        string subjectPrefix,
        string ticketSubject,
        string patientName,
        Guid patientId,
        string patientEmail,
        Guid ticketId,
        string message)
    {
        var subject = $"{subjectPrefix} {ticketSubject}".Trim();
        var emailDisplay = string.IsNullOrWhiteSpace(patientEmail) ? "(none)" : patientEmail.Trim();
        var plain =
            $"""
            Patient: {patientName} ({patientId:D})
            Email: {emailDisplay}
            Ticket: {ticketId:D}

            {message}
            """;

        var html = RaphCareEmailLayout.Wrap(
            RaphCareEmailLayout.Heading("New support message")
            + RaphCareEmailLayout.Paragraph("A patient sent a message from the RaphCare app.")
            + RaphCareEmailLayout.InfoBox(
            [
                ("Patient", $"{patientName} ({patientId:D})"),
                ("Email", emailDisplay),
                ("Subject", ticketSubject),
                ("Ticket", ticketId.ToString("D"))
            ])
            + RaphCareEmailLayout.MessageBox(message),
            preheader: $"{patientName}: {ticketSubject}");

        return new EmailContent(subject, plain, html);
    }
}
