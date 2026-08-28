namespace RaphCare.Client.Models.Support;

public sealed class PatientSupportContentViewModel
{
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhoneE164 { get; set; } = string.Empty;
    public string SupportPhoneDisplay { get; set; } = string.Empty;
    public IReadOnlyList<PatientSupportFaqItemViewModel> Faq { get; set; } = Array.Empty<PatientSupportFaqItemViewModel>();
}

public sealed class PatientSupportFaqItemViewModel
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public sealed class SubmitPatientSupportMessageRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
