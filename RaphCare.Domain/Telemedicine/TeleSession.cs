using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Telemedicine;

/// <summary>
/// Live telemedicine consultation. Aggregate root; legally sensitive.
/// </summary>
public class TeleSession : AggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public string Status { get; set; } = string.Empty; // Scheduled / InProgress / Completed / Cancelled
    public string Platform { get; set; } = string.Empty; // Zoom / WebRTC / Teams / Custom
    public string? SessionExternalId { get; set; }
    public bool IsRecorded { get; set; }
    public string? RecordingUrl { get; set; }
    public bool IsSecure { get; set; }

    public ICollection<TeleSessionParticipant> TeleSessionParticipants { get; set; } = new List<TeleSessionParticipant>();
    public ICollection<TeleSessionChat> TeleSessionChats { get; set; } = new List<TeleSessionChat>();
    public ICollection<TeleSessionEventLog> TeleSessionEventLogs { get; set; } = new List<TeleSessionEventLog>();
    public ICollection<TeleSessionRecording> TeleSessionRecordings { get; set; } = new List<TeleSessionRecording>();
    public PreVisitQuestionnaire? PreVisitQuestionnaire { get; set; }
    public PostVisitSummary? PostVisitSummary { get; set; }
}
