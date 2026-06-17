using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence.Services;

/// <summary>Loads <see cref="PatientPushDevice"/> rows for FCM/APNs delivery.</summary>
public sealed class PatientPushDeviceTokenReader(IRepository<PatientPushDevice> devices) : IPatientPushDeviceTokenReader
{
    private readonly IRepository<PatientPushDevice> _devices = devices;

    /// <inheritdoc />
    public async Task<IReadOnlyList<PatientPushDeviceToken>> GetTokensForPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var page = await _devices.SearchAsync(
                q => q.Where(d => d.PatientId == patientId && d.DeviceToken != string.Empty),
                pageNumber: 1,
                pageSize: 500,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        return page.Items
            .Select(d => new PatientPushDeviceToken(d.DeviceToken.Trim(), d.Platform.Trim()))
            .Where(t => t.DeviceToken.Length > 0)
            .ToList();
    }
}
