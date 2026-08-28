namespace RaphCare.Application.Common.Configuration;

/// <summary>Patient help &amp; support settings bound from <c>PatientSupport</c> configuration.</summary>
public sealed class PatientSupportOptions
{
    public const string SectionName = "PatientSupport";

    public string SupportEmail { get; set; } = "support@raphcare.local";
    public string SupportPhoneE164 { get; set; } = "+27123456789";
    public string SupportPhoneDisplay { get; set; } = "+27 12 345 6789";
    public string InboundEmailSubjectPrefix { get; set; } = "[RaphCare Support]";
    public PatientSupportFaqItemOptions[] Faq { get; set; } = [];
}

public sealed class PatientSupportFaqItemOptions
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
