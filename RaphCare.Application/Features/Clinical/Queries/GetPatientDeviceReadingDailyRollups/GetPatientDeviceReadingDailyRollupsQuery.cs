using MediatR;
using RaphCare.Application.Features.Clinical.DTOs;

namespace RaphCare.Application.Features.Clinical.Queries.GetPatientDeviceReadingDailyRollups;

public sealed class GetPatientDeviceReadingDailyRollupsQuery : IRequest<IReadOnlyList<DeviceReadingDailyRollupDto>>
{
    public Guid PatientId { get; set; }
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
}
