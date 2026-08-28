using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissionById;

public sealed class GetAdminClinicAdmissionByIdQuery : IRequest<AdminClinicAdmissionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AdmissionId { get; set; }
}
