namespace RaphCare.Application.Features.PatientSupport.DTOs;

/// <summary>FAQ and contact details for <c>GET api/patient/support</c>.</summary>
public sealed class PatientSupportContentDto
{
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhoneE164 { get; set; } = string.Empty;
    public string SupportPhoneDisplay { get; set; } = string.Empty;
    public IReadOnlyList<PatientSupportFaqItemDto> Faq { get; set; } = Array.Empty<PatientSupportFaqItemDto>();
}

public sealed class PatientSupportFaqItemDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
