using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Reporting.DTOs;

public class DashboardSnapshotDto : BaseDto
{
    public Guid ClinicId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
}

