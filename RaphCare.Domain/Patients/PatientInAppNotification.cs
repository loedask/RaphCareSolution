using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>In-app notification row for a patient (notification center / future push pairing).</summary>
public sealed class PatientInAppNotification : BaseEntity
{
    public Guid PatientId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    /// <summary>Category for UI styling or routing, e.g. <c>appointment</c>, <c>system</c>, <c>billing</c>.</summary>
    public string Type { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public void MarkAsRead()
    {
        if (IsRead)
            return;
        IsRead = true;
        SetUpdated();
    }
}
