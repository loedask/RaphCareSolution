namespace RaphCare.Application.Common.Configuration;

/// <summary>Twilio REST API for outbound SMS. Bound from configuration section <see cref="SectionName"/>.</summary>
public class TwilioSmsOptions
{
    public const string SectionName = "Twilio";

    public string? AccountSid { get; set; }
    public string? AuthToken { get; set; }
    public string? FromPhoneE164 { get; set; }

    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(AccountSid)
        && !string.IsNullOrWhiteSpace(AuthToken)
        && !string.IsNullOrWhiteSpace(FromPhoneE164);
}
