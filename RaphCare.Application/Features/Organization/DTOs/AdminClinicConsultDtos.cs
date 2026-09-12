namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicConsultBoardDto
{
    public string ClinicName { get; set; } = string.Empty;
    public int WaitingCount { get; set; }
    public int CalledCount { get; set; }
    public IReadOnlyList<AdminClinicConsultTicketDto> Waiting { get; set; } =
        Array.Empty<AdminClinicConsultTicketDto>();
    public IReadOnlyList<AdminClinicConsultTicketDto> Called { get; set; } =
        Array.Empty<AdminClinicConsultTicketDto>();
    public IReadOnlyList<AdminClinicConsultTicketDto> Recent { get; set; } =
        Array.Empty<AdminClinicConsultTicketDto>();
    public IReadOnlyList<AdminClinicConsultAppointmentDto> TodayAppointments { get; set; } =
        Array.Empty<AdminClinicConsultAppointmentDto>();
}

public sealed class AdminClinicConsultTicketDto
{
    public Guid Id { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string QueueCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ArrivedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ScheduledStart { get; set; }
}

public sealed class AdminClinicConsultAppointmentDto
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool AlreadyQueued { get; set; }
}

public sealed class ConsultDisplayBoardDto
{
    public string ClinicName { get; set; } = string.Empty;
    public ConsultDisplayTicketDto? NowServing { get; set; }
    public IReadOnlyList<ConsultDisplayTicketDto> Waiting { get; set; } =
        Array.Empty<ConsultDisplayTicketDto>();
}

public sealed class ConsultDisplayTicketDto
{
    public string QueueCode { get; set; } = string.Empty;
}

public sealed class ConsultDisplayLinkDto
{
    public string Token { get; set; } = string.Empty;
}
