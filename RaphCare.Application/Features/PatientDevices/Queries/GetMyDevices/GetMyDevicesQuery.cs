using MediatR;
using RaphCare.Application.Features.PatientDevices.DTOs;

namespace RaphCare.Application.Features.PatientDevices.Queries.GetMyDevices;

public sealed class GetMyDevicesQuery : IRequest<IReadOnlyList<PatientDeviceListItemDto>>
{
}
