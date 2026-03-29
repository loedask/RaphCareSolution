using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.PatientTelehealth.DTOs;

public class PatientTeleSessionListItemDto : BaseDto
{
    public DateTime ScheduledStart { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
