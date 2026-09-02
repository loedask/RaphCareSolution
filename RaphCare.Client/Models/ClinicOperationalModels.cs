namespace RaphCare.Client.Models;

public sealed class ClinicDashboard
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int PatientCount { get; set; }
    public int StaffCount { get; set; }
    public int ProviderCount { get; set; }
    public int FacilityCount { get; set; }
    public int AppointmentsTodayCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int OccupancyPercent { get; set; }
    public int AdmissionsTodayCount { get; set; }
    public int DischargesTodayCount { get; set; }
    public decimal? AverageLengthOfStayDays { get; set; }
    public IReadOnlyList<ClinicAppointmentListItem> UpcomingAppointments { get; set; } = Array.Empty<ClinicAppointmentListItem>();
}

public sealed class ClinicProviderListItem
{
    public Guid ProviderId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ScheduleSlotCount { get; set; }
}

public sealed class ClinicProviderDetail
{
    public Guid ProviderId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyList<ClinicProviderSchedule> Schedules { get; set; } = Array.Empty<ClinicProviderSchedule>();
}

public sealed class ClinicProviderSchedule
{
    public Guid Id { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
}

public sealed class ClinicAppointmentListItem
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? ActiveVisitId { get; set; }
}

public sealed class PagedClinicAppointments
{
    public IReadOnlyList<ClinicAppointmentListItem> Items { get; set; } = Array.Empty<ClinicAppointmentListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public sealed class CreateProviderScheduleRequest
{
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public sealed class BookClinicAppointmentRequest
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = "InPerson";
    public string? Reason { get; set; }
}

public sealed class RescheduleClinicAppointmentRequest
{
    public Guid? ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? Reason { get; set; }
}

public sealed class ClinicPatientAppointmentSummary
{
    public Guid Id { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string ProviderName { get; set; } = string.Empty;
}

public sealed class ClinicPatientVitalSummary
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
    public string? VisitType { get; set; }
}

public sealed class ClinicPatientDeviceReadingSummary
{
    public string Kind { get; set; } = string.Empty;
    public string ReadingType { get; set; } = string.Empty;
    public decimal PrimaryValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public int? HeartRateBpm { get; set; }
    public decimal? SpO2Percent { get; set; }
}

public sealed class ClinicPatientDeviceRollupSummary
{
    public DateOnly Date { get; set; }
    public int HeartRateSampleCount { get; set; }
    public decimal? AvgHeartRateBpm { get; set; }
    public int? MinHeartRateBpm { get; set; }
    public int? MaxHeartRateBpm { get; set; }
    public int SpO2SampleCount { get; set; }
    public decimal? AvgSpO2Percent { get; set; }
    public decimal? MinSpO2Percent { get; set; }
    public decimal? MaxSpO2Percent { get; set; }
}

public sealed class ClinicVisitDetail
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public IReadOnlyList<ClinicVisitVital> Vitals { get; set; } = Array.Empty<ClinicVisitVital>();
    public IReadOnlyList<ClinicVisitDiagnosis> Diagnoses { get; set; } = Array.Empty<ClinicVisitDiagnosis>();
    public IReadOnlyList<ClinicVisitPrescription> Prescriptions { get; set; } = Array.Empty<ClinicVisitPrescription>();
    public IReadOnlyList<ClinicVisitNote> ClinicalNotes { get; set; } = Array.Empty<ClinicVisitNote>();
    public IReadOnlyList<ClinicVisitSoapNote> SoapNotes { get; set; } = Array.Empty<ClinicVisitSoapNote>();
    public IReadOnlyList<ClinicVisitLabResult> LabResults { get; set; } = Array.Empty<ClinicVisitLabResult>();
}

public sealed class ClinicVisitVital
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
}

public sealed class ClinicPatientMedicalSummary
{
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? Medications { get; set; }
    public string? PrimaryDoctor { get; set; }
    public IReadOnlyList<ClinicPatientAllergy> RecordedAllergies { get; set; } = Array.Empty<ClinicPatientAllergy>();
    public IReadOnlyList<ClinicPatientMedication> RecordedMedications { get; set; } = Array.Empty<ClinicPatientMedication>();
}

public sealed class ClinicPatientAllergy
{
    public string Substance { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public string? Severity { get; set; }
    public DateTime RecordedAt { get; set; }
}

public sealed class ClinicPatientMedication
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class ClinicPatientEmergencyContact
{
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public sealed class ClinicPatientInsuranceProfile
{
    public string PlanName { get; set; } = string.Empty;
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ClinicPatientInvoice
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? VisitId { get; set; }
}

public sealed class ClinicPatientMoodLog
{
    public DateTime LoggedAt { get; set; }
    public int MoodScore { get; set; }
    public string? Notes { get; set; }
    public bool IsFlagged { get; set; }
}

public sealed class ClinicPatientCarePlan
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class ClinicVisitDiagnosis
{
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Severity { get; set; }
}

public sealed class ClinicVisitPrescription
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime IssuedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public string PickupCode { get; set; } = string.Empty;
    public DateTime? DispensedAt { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<ClinicVisitPrescriptionItem> Items { get; set; } = Array.Empty<ClinicVisitPrescriptionItem>();
}

public sealed class ClinicVisitPrescriptionItem
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }
}

public sealed class ClinicVisitNote
{
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public sealed class ClinicVisitSoapNote
{
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
}

public sealed class ClinicVisitLabResult
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public DateTime VisitStart { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string PickupCode { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string? ResultValue { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public DateTime? ReportedAt { get; set; }
}

public sealed class RecordVisitVitalRequest
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime? RecordedAt { get; set; }
}

public sealed class SaveVisitSoapNoteRequest
{
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
}

public sealed class AddVisitNoteRequest
{
    public string Notes { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public sealed class AddVisitPrescriptionRequest
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<ClinicVisitPrescriptionItem> Items { get; set; } =
        Array.Empty<ClinicVisitPrescriptionItem>();
}

public sealed class AddVisitLabResultRequest
{
    public string TestName { get; set; } = string.Empty;
    public string? Priority { get; set; }
}

public sealed class CompleteVisitLabOrderRequest
{
    public string ResultValue { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
}

public sealed class ClinicCollectionBoard
{
    public IReadOnlyList<ClinicCollectionPrescription> Prescriptions { get; set; } =
        Array.Empty<ClinicCollectionPrescription>();

    public IReadOnlyList<ClinicCollectionLabOrder> LabOrders { get; set; } =
        Array.Empty<ClinicCollectionLabOrder>();

    public IReadOnlyList<ClinicCollectionPrescription> RecentPrescriptions { get; set; } =
        Array.Empty<ClinicCollectionPrescription>();

    public IReadOnlyList<ClinicCollectionLabOrder> RecentLabOrders { get; set; } =
        Array.Empty<ClinicCollectionLabOrder>();
}

public sealed class ClinicCollectionPrescription
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? NationalHealthId { get; set; }
    public string PickupCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? DispensedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<ClinicVisitPrescriptionItem> Items { get; set; } =
        Array.Empty<ClinicVisitPrescriptionItem>();
}

public sealed class ClinicCollectionLabOrder
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? NationalHealthId { get; set; }
    public string PickupCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAt { get; set; }
    public DateTime? CalledAt { get; set; }
}

public sealed class ClinicDeviceListItem
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsAssigned { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedPatientId { get; set; }
    public string? AssignedPatientName { get; set; }
}

public sealed class ClinicInpatientBoard
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
    public IReadOnlyList<ClinicWard> Wards { get; set; } = Array.Empty<ClinicWard>();
    public IReadOnlyList<ClinicAdmission> ActiveAdmissionsList { get; set; } = Array.Empty<ClinicAdmission>();
}

public sealed class ClinicWard
{
    public Guid Id { get; set; }
    public Guid FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<ClinicRoom> Rooms { get; set; } = Array.Empty<ClinicRoom>();
}

public sealed class ClinicRoom
{
    public Guid Id { get; set; }
    public Guid WardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<ClinicBed> Beds { get; set; } = Array.Empty<ClinicBed>();
}

public sealed class ClinicBed
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

public sealed class ClinicAdmission
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

public sealed class ClinicAdmissionObservation
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

public sealed class DischargeAdmissionRequest
{
    public string? Notes { get; set; }
    public string? DischargeSummary { get; set; }
    public decimal? NightlyBedRate { get; set; }
    public decimal? ExtraAmount { get; set; }
    public string? ExtraDescription { get; set; }
    public bool MarkPaid { get; set; }
    public string? Currency { get; set; }
    public bool BookReturnVisit { get; set; }
    public Guid? ReturnProviderId { get; set; }
    public DateTime? ReturnScheduledStart { get; set; }
    public DateTime? ReturnScheduledEnd { get; set; }
    public string? ReturnAppointmentType { get; set; }
    public string? ReturnReason { get; set; }
}

public sealed class AddAdmissionObservationRequest
{
    public string Note { get; set; } = string.Empty;
    public decimal? HeartRate { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public decimal? SystolicBp { get; set; }
    public decimal? DiastolicBp { get; set; }
}

public sealed class CreateWardRequest
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public sealed class CreateRoomRequest
{
    public Guid WardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
}

public sealed class CreateBedRequest
{
    public Guid RoomId { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class AdmitPatientRequest
{
    public Guid PatientId { get; set; }
    public Guid BedId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateWardRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateBedRequest
{
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class SetBedStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class TransferAdmissionRequest
{
    public Guid TargetBedId { get; set; }
    public string? Notes { get; set; }
}

public sealed class PagedClinicAdmissions
{
    public IReadOnlyList<ClinicAdmission> Items { get; set; } = Array.Empty<ClinicAdmission>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public sealed class ClinicTeleJoinInfo
{
    public Guid TeleSessionId { get; set; }
    public Guid VisitId { get; set; }
    public Guid AppointmentId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public uint Uid { get; set; }
    public string? AppId { get; set; }
    public string? RtcToken { get; set; }
    public long TokenExpiresAtUnix { get; set; }
    public bool RtcConfigured { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string? PatientName { get; set; }
    public string? ProviderDisplayName { get; set; }
}

public sealed class ClinicCasualtyBoard
{
    public string ClinicName { get; set; } = string.Empty;
    public int WaitingCount { get; set; }
    public int CalledCount { get; set; }
    public IReadOnlyList<ClinicCasualtyTicket> Waiting { get; set; } =
        Array.Empty<ClinicCasualtyTicket>();
    public IReadOnlyList<ClinicCasualtyTicket> Called { get; set; } =
        Array.Empty<ClinicCasualtyTicket>();
    public IReadOnlyList<ClinicCasualtyTicket> Recent { get; set; } =
        Array.Empty<ClinicCasualtyTicket>();
}

public sealed class ClinicCasualtyTicket
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

public sealed class CreateCasualtyTicketRequest
{
    public Guid? PatientId { get; set; }
    public string TriageLevel { get; set; } = "Green";
    public string? ChiefComplaint { get; set; }
}

public sealed class ClinicTheatreBoard
{
    public string ClinicName { get; set; } = string.Empty;
    public DateTime DayUtc { get; set; }
    public int ScheduledCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public IReadOnlyList<ClinicTheatreCase> Cases { get; set; } =
        Array.Empty<ClinicTheatreCase>();
}

public sealed class ClinicTheatreCase
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

public sealed class CreateTheatreCaseRequest
{
    public Guid PatientId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string? TheatreName { get; set; }
    public string? SurgeonName { get; set; }
    public string? Notes { get; set; }
}

public sealed class ClinicReferralBoard
{
    public string ClinicName { get; set; } = string.Empty;
    public int SentCount { get; set; }
    public int AcceptedCount { get; set; }
    public int CompletedCount { get; set; }
    public IReadOnlyList<ClinicReferral> Open { get; set; } =
        Array.Empty<ClinicReferral>();
    public IReadOnlyList<ClinicReferral> Recent { get; set; } =
        Array.Empty<ClinicReferral>();
}

public sealed class ClinicReferral
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? VisitId { get; set; }
    public string ReferredTo { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Specialty { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReferredAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public sealed class CreateReferralRequest
{
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public string ReferredTo { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Specialty { get; set; }
    public string? Notes { get; set; }
}

public sealed class ClinicRosterBoard
{
    public string ClinicName { get; set; } = string.Empty;
    public DateTime DayUtc { get; set; }
    public int MorningCount { get; set; }
    public int AfternoonCount { get; set; }
    public int NightCount { get; set; }
    public IReadOnlyList<ClinicRosterEntry> Entries { get; set; } =
        Array.Empty<ClinicRosterEntry>();
}

public sealed class ClinicRosterEntry
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

public sealed class CreateRosterEntryRequest
{
    public Guid ApplicationUserId { get; set; }
    public DateTime DutyDate { get; set; }
    public string ShiftLabel { get; set; } = "Morning";
    public string? Note { get; set; }
}

