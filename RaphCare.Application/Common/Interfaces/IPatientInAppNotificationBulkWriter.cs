namespace RaphCare.Application.Common.Interfaces;

/// <summary>Bulk updates for patient in-app notifications (EF <c>ExecuteUpdate</c>).</summary>
public interface IPatientInAppNotificationBulkWriter
{
    /// <summary>Marks every unread notification for the patient as read.</summary>
    /// <returns>Number of rows updated.</returns>
    Task<int> MarkAllReadForPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
}
