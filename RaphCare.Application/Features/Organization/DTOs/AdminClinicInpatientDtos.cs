namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicInpatientBoardDto
{
    public Guid ClinicId { get; set; }
    public int TotalBeds { get; set; }
    public int AvailableBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int MaintenanceBeds { get; set; }
    public int ActiveAdmissions { get; set; }
    public int OccupancyPercent { get; set; }
    public int AdmissionsTodayCount { get; set; }
    public int DischargesTodayCount { get; set; }
    public decimal? AverageLengthOfStayDays { get; set; }
    public IReadOnlyList<AdminClinicWardDto> Wards { get; set; } = Array.Empty<AdminClinicWardDto>();
    public IReadOnlyList<AdminClinicAdmissionDto> ActiveAdmissionsList { get; set; } = Array.Empty<AdminClinicAdmissionDto>();
}

public sealed class AdminClinicWardDto
{
    public Guid Id { get; set; }
    public Guid FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<AdminClinicRoomDto> Rooms { get; set; } = Array.Empty<AdminClinicRoomDto>();
}

public sealed class AdminClinicRoomDto
{
    public Guid Id { get; set; }
    public Guid WardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<AdminClinicBedDto> Beds { get; set; } = Array.Empty<AdminClinicBedDto>();
}

public sealed class AdminClinicBedDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid? CurrentAdmissionId { get; set; }
    public Guid? CurrentPatientId { get; set; }
    public string? CurrentPatientName { get; set; }
}

public sealed class AdminClinicAdmissionDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid BedId { get; set; }
    public string BedLabel { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string WardName { get; set; } = string.Empty;
    public string FacilityName { get; set; } = string.Empty;
    public DateTime AdmittedAt { get; set; }
    public DateTime? DischargedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? DischargeSummary { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal? InvoiceAmount { get; set; }
    public string? InvoiceCurrency { get; set; }
    public string? InvoiceStatus { get; set; }
    public DateTime? InvoicePaidAt { get; set; }
    public int? BedNights { get; set; }
    public Guid? ReturnAppointmentId { get; set; }
    public DateTime? ReturnAppointmentStart { get; set; }
    public DateTime? ReturnAppointmentEnd { get; set; }
}

public sealed class AdminClinicDischargeSummaryDraftDto
{
    public string DraftText { get; set; } = string.Empty;
}

public sealed class AdminClinicAdmissionObservationDto
{
    public Guid Id { get; set; }
    public Guid AdmissionId { get; set; }
    public DateTime RecordedAt { get; set; }
    public string Note { get; set; } = string.Empty;
    public decimal? HeartRate { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public decimal? SystolicBp { get; set; }
    public decimal? DiastolicBp { get; set; }
}
