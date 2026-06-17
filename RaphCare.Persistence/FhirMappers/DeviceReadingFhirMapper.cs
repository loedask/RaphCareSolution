using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Persistence.FhirMappers;

/// <summary>LOINC-coded Observations for heart rate and pulse-ox readings from the device context.</summary>
public sealed class DeviceReadingFhirMapper : IDeviceReadingFhirMapper
{
    private const string Loinc = "http://loinc.org";
    private const string ObservationCategory = "http://terminology.hl7.org/CodeSystem/observation-category";

    public Task<FhirObservationDto> MapToObservationAsync(DeviceReading reading, Device _, CancellationToken ct)
    {
        var subjectRef = $"Patient/{reading.PatientId}";
        var dto = new FhirObservationDto
        {
            Id = reading.Id.ToString(),
            Subject = new FhirReferenceDto { Reference = subjectRef },
            EffectiveDateTime = new DateTimeOffset(DateTime.SpecifyKind(reading.RecordedAt, DateTimeKind.Utc)),
            Issued = new DateTimeOffset(DateTime.SpecifyKind(reading.ReceivedAt, DateTimeKind.Utc)),
            Category =
            [
                new FhirCodeableConceptDto
                {
                    Coding =
                    [
                        new FhirCodingDto
                        {
                            System = ObservationCategory,
                            Code = "vital-signs",
                            Display = "Vital Signs"
                        }
                    ]
                }
            ]
        };

        switch (reading)
        {
            case HeartRateReading hr:
                dto.Code = HeartRateCode();
                dto.ValueQuantity = new FhirQuantityDto
                {
                    Value = hr.HeartRate,
                    Unit = "beats/minute",
                    Code = "/min"
                };
                break;

            case PulseOximeterReading po:
                dto.Code = SpO2Code();
                dto.ValueQuantity = new FhirQuantityDto
                {
                    Value = po.SpO2,
                    Unit = "%",
                    Code = "%"
                };
                if (po.PulseRate > 0)
                {
                    dto.Component =
                    [
                        new FhirObservationComponentDto
                        {
                            Code = HeartRateCode(),
                            ValueQuantity = new FhirQuantityDto
                            {
                                Value = po.PulseRate,
                                Unit = "beats/minute",
                                Code = "/min"
                            }
                        }
                    ];
                }

                break;

            default:
                dto.Code = GenericCode(reading.ReadingType);
                dto.ValueQuantity = new FhirQuantityDto
                {
                    Value = reading.PrimaryValue,
                    Unit = string.IsNullOrEmpty(reading.Unit) ? null : reading.Unit,
                    Code = null
                };
                break;
        }

        return Task.FromResult(dto);
    }

    private static FhirCodeableConceptDto HeartRateCode() =>
        new()
        {
            Coding =
            [
                new FhirCodingDto
                {
                    System = Loinc,
                    Code = "8867-4",
                    Display = "Heart rate"
                }
            ],
            Text = "Heart rate"
        };

    private static FhirCodeableConceptDto SpO2Code() =>
        new()
        {
            Coding =
            [
                new FhirCodingDto
                {
                    System = Loinc,
                    Code = "59408-5",
                    Display = "Oxygen saturation in Arterial blood by Pulse oximetry"
                }
            ],
            Text = "SpO2"
        };

    private static FhirCodeableConceptDto GenericCode(string readingType) =>
        new()
        {
            Text = string.IsNullOrWhiteSpace(readingType) ? "Device reading" : readingType
        };
}
