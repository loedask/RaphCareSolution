using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.RegisterClinic;

/// <summary>Platform use case: onboard a new hospital (clinic) with an optional primary facility.</summary>
public sealed class RegisterClinicCommand : IRequest<RegisterClinicResultDto>
{
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;

    public string? FacilityName { get; set; }
    public string? FacilityAddress { get; set; }
    public string? FacilityCity { get; set; }
    public bool IsVirtualFacility { get; set; }
}
