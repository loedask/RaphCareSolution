using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Clinical.DTOs;

namespace RaphCare.Application.Features.Clinical.Queries.GetPatientDeviceReadingDailyRollups;

public sealed class GetPatientDeviceReadingDailyRollupsHandler(
    IClinicContext clinicContext,
    IDeviceReadingRollupService rollupService) : IRequestHandler<GetPatientDeviceReadingDailyRollupsQuery, IReadOnlyList<DeviceReadingDailyRollupDto>>
{
    private readonly IClinicContext _clinicContext = clinicContext;
    private readonly IDeviceReadingRollupService _rollupService = rollupService;

    public async Task<IReadOnlyList<DeviceReadingDailyRollupDto>> Handle(
        GetPatientDeviceReadingDailyRollupsQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = _clinicContext.ClinicId
            ?? throw new ForbiddenAccessException("Missing clinic context.");

        return await _rollupService.GetDailyRollupsAsync(
            request.PatientId,
            clinicId,
            request.FromUtc,
            request.ToUtc,
            cancellationToken).ConfigureAwait(false);
    }
}
