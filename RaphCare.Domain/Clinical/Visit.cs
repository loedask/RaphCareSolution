using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Actual clinical encounter. Aggregate root; legally sensitive.
/// </summary>
public class Visit : AggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public ICollection<VisitStatusHistory> VisitStatusHistories { get; set; } = new List<VisitStatusHistory>();
    public ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<LabRequest> LabRequests { get; set; } = new List<LabRequest>();
    public ICollection<VitalSignRecord> VitalSignRecords { get; set; } = new List<VitalSignRecord>();
    public ICollection<ClinicalAttachment> ClinicalAttachments { get; set; } = new List<ClinicalAttachment>();
    public SOAPNote? SOAPNote { get; set; }
    public ICollection<FollowUpInstruction> FollowUpInstructions { get; set; } = new List<FollowUpInstruction>();
}
