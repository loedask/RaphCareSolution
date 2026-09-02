namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicCasualtyBoardDto
{
    public string ClinicName { get; set; } = string.Empty;
    public int WaitingCount { get; set; }
    public int CalledCount { get; set; }
    public IReadOnlyList<AdminClinicCasualtyTicketDto> Waiting { get; set; } =
        Array.Empty<AdminClinicCasualtyTicketDto>();
    public IReadOnlyList<AdminClinicCasualtyTicketDto> Called { get; set; } =
        Array.Empty<AdminClinicCasualtyTicketDto>();
    public IReadOnlyList<AdminClinicCasualtyTicketDto> Recent { get; set; } =
        Array.Empty<AdminClinicCasualtyTicketDto>();
}

public sealed class AdminClinicCasualtyTicketDto
{
    public Guid Id { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string QueueCode { get; set; } = string.Empty;
    public string TriageLevel { get; set; } = string.Empty;
    public string? ChiefComplaint { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ArrivedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public sealed class CasualtyDisplayBoardDto
{
    public string ClinicName { get; set; } = string.Empty;
    public CasualtyDisplayTicketDto? NowServing { get; set; }
    public IReadOnlyList<CasualtyDisplayTicketDto> Waiting { get; set; } =
        Array.Empty<CasualtyDisplayTicketDto>();
}

public sealed class CasualtyDisplayTicketDto
{
    public string QueueCode { get; set; } = string.Empty;
    public string TriageLevel { get; set; } = string.Empty;
}

public sealed class CasualtyDisplayLinkDto
{
    public string Token { get; set; } = string.Empty;
}

public sealed class AdminClinicTheatreBoardDto
{
    public string ClinicName { get; set; } = string.Empty;
    public DateTime DayUtc { get; set; }
    public int ScheduledCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public IReadOnlyList<AdminClinicTheatreCaseDto> Cases { get; set; } =
        Array.Empty<AdminClinicTheatreCaseDto>();
}

public sealed class AdminClinicTheatreCaseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string? TheatreName { get; set; }
    public string? SurgeonName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
