using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAdmissionObservation;

public sealed class CreateAdminClinicAdmissionObservationCommand : IRequest<AdminClinicAdmissionObservationDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AdmissionId { get; set; }
    public string Note { get; set; } = string.Empty;
    public decimal? HeartRate { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public decimal? SystolicBp { get; set; }
    public decimal? DiastolicBp { get; set; }
}
