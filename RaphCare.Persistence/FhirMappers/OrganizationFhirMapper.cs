using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Persistence.FhirMappers;

/// <summary>
/// Minimal FHIR-shaped Organization mapper (phase 1).
/// </summary>
public class OrganizationFhirMapper : IOrganizationFhirMapper
{
    public Task<FhirOrganizationDto> MapToDtoAsync(Clinic organization, CancellationToken ct)
    {
        var dto = new FhirOrganizationDto
        {
            Id = organization.Id.ToString(),
            Name = string.IsNullOrWhiteSpace(organization.Name) ? null : organization.Name,
            Active = organization.IsActive
        };

        if (!string.IsNullOrWhiteSpace(organization.RegistrationNumber))
        {
            dto.Identifier.Add(new FhirIdentifierDto
            {
                System = "urn:raphcare:clinic-registration-number",
                Value = organization.RegistrationNumber
            });
        }

        return Task.FromResult(dto);
    }
}

