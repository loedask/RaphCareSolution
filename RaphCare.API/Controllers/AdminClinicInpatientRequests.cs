namespace RaphCare.API.Controllers;

public sealed class CreateAdminClinicWardRequest
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public sealed class UpdateAdminClinicWardRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CreateAdminClinicRoomRequest
{
    public Guid WardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
}

public sealed class UpdateAdminClinicRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CreateAdminClinicBedRequest
{
    public Guid RoomId { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class UpdateAdminClinicBedRequest
{
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class SetAdminClinicBedStatusRequest
{
    public string Status { get; set; } = string.Empty;
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
    public string? DischargeSummary { get; set; }
    public decimal? NightlyBedRate { get; set; }
    public decimal? ExtraAmount { get; set; }
    public string? ExtraDescription { get; set; }
    public bool MarkPaid { get; set; }
    public string? Currency { get; set; }
}

public sealed class CreateAdminClinicAdmissionObservationRequest
{
    public string Note { get; set; } = string.Empty;
    public decimal? HeartRate { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public decimal? SystolicBp { get; set; }
    public decimal? DiastolicBp { get; set; }
}

public sealed class TransferAdminClinicAdmissionRequest
{
    public Guid TargetBedId { get; set; }
    public string? Notes { get; set; }
}
