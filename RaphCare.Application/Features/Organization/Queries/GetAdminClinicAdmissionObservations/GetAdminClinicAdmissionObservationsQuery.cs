using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissionObservations;

public sealed class GetAdminClinicAdmissionObservationsQuery : IRequest<IReadOnlyList<AdminClinicAdmissionObservationDto>?>
{
    public Guid ClinicId { get; set; }
    public Guid AdmissionId { get; set; }
}
