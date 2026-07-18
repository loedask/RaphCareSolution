namespace RaphCare.API.Controllers;

public sealed class CreateAdminClinicWardRequest
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public sealed class CreateAdminClinicRoomRequest
{
    public Guid WardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
}

public sealed class CreateAdminClinicBedRequest
{
    public Guid RoomId { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class AdmitAdminClinicPatientRequest
{
    public Guid PatientId { get; set; }
    public Guid BedId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public sealed class DischargeAdminClinicAdmissionRequest
{
    public string? Notes { get; set; }
}
