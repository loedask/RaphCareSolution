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

public sealed class CreateAdminClinicCasualtyTicketRequest
{
    public Guid? PatientId { get; set; }
    public string TriageLevel { get; set; } = "Green";
    public string? ChiefComplaint { get; set; }
}

public sealed class CompleteAdminClinicCasualtyTicketRequest
{
    public bool Cancel { get; set; }
}

public sealed class CreateAdminClinicTheatreCaseRequest
{
    public Guid PatientId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string? TheatreName { get; set; }
    public string? SurgeonName { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateAdminClinicTheatreCaseStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class CreateAdminClinicReferralRequest
{
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public string ReferredTo { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Specialty { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateAdminClinicReferralStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

