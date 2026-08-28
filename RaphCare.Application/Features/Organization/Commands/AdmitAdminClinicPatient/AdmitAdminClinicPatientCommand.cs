using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.AdmitAdminClinicPatient;

public sealed class AdmitAdminClinicPatientCommand : IRequest<AdminClinicAdmissionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid BedId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
