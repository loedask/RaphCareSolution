namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicRosterBoardDto
{
    public string ClinicName { get; set; } = string.Empty;
    public DateTime DayUtc { get; set; }
    public int MorningCount { get; set; }
    public int AfternoonCount { get; set; }
    public int NightCount { get; set; }
    public IReadOnlyList<AdminClinicRosterEntryDto> Entries { get; set; } =
        Array.Empty<AdminClinicRosterEntryDto>();
}

public sealed class AdminClinicRosterEntryDto
{
    public Guid Id { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public DateTime DutyDate { get; set; }
    public string ShiftLabel { get; set; } = string.Empty;
    public string? Note { get; set; }
}
