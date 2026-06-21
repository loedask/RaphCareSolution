using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatientById;

public sealed class GetAdminClinicPatientByIdQuery : IRequest<AdminClinicPatientDetailDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
}
