using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Patients;
using System.Globalization;

namespace RaphCare.Persistence.FhirMappers;

/// <summary>
/// Minimal FHIR-shaped patient mapper (phase 1).
/// </summary>
public class PatientFhirMapper(ClinicalDbContext clinicalDbContext) : IPatientFhirMapper
{
    private readonly ClinicalDbContext _clinicalDbContext = clinicalDbContext ?? throw new ArgumentNullException(nameof(clinicalDbContext));

    public async Task<FhirPatientDto> MapToDtoAsync(Patient patient, CancellationToken ct)
    {
        var externalIds = await _clinicalDbContext.PatientExternalIds
            .Where(e => e.PatientId == patient.Id)
            .Select(e => new { e.SourceSystem, e.ExternalId })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var dto = new FhirPatientDto
        {
            Id = patient.Id.ToString(),
            Gender = string.IsNullOrWhiteSpace(patient.Gender) ? null : patient.Gender,
            BirthDate = patient.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Name = new List<FhirHumanNameDto>
            {
                new()
                {
                    Family = string.IsNullOrWhiteSpace(patient.LastName) ? null : patient.LastName,
                    Given = string.IsNullOrWhiteSpace(patient.FirstName)
                        ? new List<string>()
                        : new List<string> { patient.FirstName }
                }
            }
        };

        // identifiers
        dto.Identifier.Add(new FhirIdentifierDto
        {
            System = "urn:raphcare:patient-id",
            Value = patient.Id.ToString()
        });

        if (!string.IsNullOrWhiteSpace(patient.NationalHealthId))
        {
            dto.Identifier.Add(new FhirIdentifierDto
            {
                System = "urn:raphcare:national-health-id",
                Value = patient.NationalHealthId
            });
        }

        if (!string.IsNullOrWhiteSpace(patient.NationalIdNumber))
        {
            dto.Identifier.Add(new FhirIdentifierDto
            {
                System = "urn:raphcare:national-id",
                Value = patient.NationalIdNumber
            });
        }

        foreach (var ext in externalIds)
        {
            if (string.IsNullOrWhiteSpace(ext.SourceSystem) || string.IsNullOrWhiteSpace(ext.ExternalId))
                continue;

            dto.Identifier.Add(new FhirIdentifierDto
            {
                System = ext.SourceSystem,
                Value = ext.ExternalId
            });
        }

        // telecom
        if (!string.IsNullOrWhiteSpace(patient.PhoneNumber))
        {
            dto.Telecom.Add(new FhirContactPointDto
            {
                System = "phone",
                Value = patient.PhoneNumber
            });
        }

        if (!string.IsNullOrWhiteSpace(patient.Email))
        {
            dto.Telecom.Add(new FhirContactPointDto
            {
                System = "email",
                Value = patient.Email
            });
        }

        return dto;
    }
}

