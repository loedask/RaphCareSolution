using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientMentalHealth.DTOs;
using RaphCare.Infrastructure.Configuration;

namespace RaphCare.Infrastructure.Services;

/// <summary>Maps <see cref="PatientMentalHealthContentOptions"/> to the patient hub DTO.</summary>
public sealed class OptionsPatientMentalHealthContentProvider(IOptions<PatientMentalHealthContentOptions> options)
    : IPatientMentalHealthContentProvider
{
    private readonly IOptions<PatientMentalHealthContentOptions> _options = options;

    public Task<PatientMentalHealthContentDto> GetContentAsync(CancellationToken cancellationToken = default)
    {
        var o = _options.Value;
        var dto = new PatientMentalHealthContentDto
        {
            InsightTitle = o.InsightTitle,
            InsightBody = o.InsightBody,
            MedicalDisclaimer = o.MedicalDisclaimer
        };
        return Task.FromResult(dto);
    }
}
