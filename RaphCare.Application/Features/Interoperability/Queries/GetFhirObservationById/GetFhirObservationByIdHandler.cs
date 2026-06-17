using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirObservationById;

public sealed class GetFhirObservationByIdHandler(
    IRepository<DeviceReading> readings,
    IClinicContext clinicContext,
    IDeviceReadingFhirMapper mapper) : IRequestHandler<GetFhirObservationByIdQuery, FhirObservationDto>
{
    private readonly IRepository<DeviceReading> _readings = readings;
    private readonly IClinicContext _clinicContext = clinicContext;
    private readonly IDeviceReadingFhirMapper _mapper = mapper;

    public async Task<FhirObservationDto> Handle(GetFhirObservationByIdQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _clinicContext.ClinicId
            ?? throw new ForbiddenAccessException("Missing clinic context.");

        var paged = await _readings.SearchAsync(
            q => q.Where(r => r.Id == request.Id).Include(r => r.Device),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var row = paged.Items.FirstOrDefault();
        if (row?.Device is null || row.Device.ClinicId != clinicId)
            throw new NotFoundException(nameof(DeviceReading), request.Id);

        return await _mapper.MapToObservationAsync(row, row.Device, cancellationToken).ConfigureAwait(false);
    }
}
