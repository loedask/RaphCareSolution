namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicReferralBoardDto
{
    public string ClinicName { get; set; } = string.Empty;
    public int SentCount { get; set; }
    public int AcceptedCount { get; set; }
    public int CompletedCount { get; set; }
    public IReadOnlyList<AdminClinicReferralDto> Open { get; set; } =
        Array.Empty<AdminClinicReferralDto>();
    public IReadOnlyList<AdminClinicReferralDto> Recent { get; set; } =
        Array.Empty<AdminClinicReferralDto>();
}

public sealed class AdminClinicReferralDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? VisitId { get; set; }
    public string ReferredTo { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Specialty { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReferredAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
