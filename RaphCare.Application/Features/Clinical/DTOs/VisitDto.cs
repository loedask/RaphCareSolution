using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Clinical.DTOs;

public class VisitDto : BaseDto
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

