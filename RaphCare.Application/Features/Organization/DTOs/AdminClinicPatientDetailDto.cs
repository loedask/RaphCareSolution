namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicPatientDetailDto
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; }
    public string GrantedByRule { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public IReadOnlyList<AdminClinicPatientVisitDto> RecentVisits { get; set; } = Array.Empty<AdminClinicPatientVisitDto>();
    public IReadOnlyList<AdminClinicPatientAppointmentDto> Appointments { get; set; } = Array.Empty<AdminClinicPatientAppointmentDto>();
}

public sealed class AdminClinicPatientAppointmentDto
{
    public Guid Id { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string ProviderName { get; set; } = string.Empty;
}

public sealed class AdminClinicPatientVisitDto
{
    public Guid Id { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
}
