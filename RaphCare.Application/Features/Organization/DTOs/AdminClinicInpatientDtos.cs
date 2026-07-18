namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicInpatientBoardDto
{
    public Guid ClinicId { get; set; }
    public int TotalBeds { get; set; }
    public int AvailableBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int ActiveAdmissions { get; set; }
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
}
