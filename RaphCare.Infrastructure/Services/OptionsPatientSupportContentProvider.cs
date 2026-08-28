using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientSupport.DTOs;

namespace RaphCare.Infrastructure.Services;

/// <summary>Maps <see cref="PatientSupportOptions"/> to patient help DTOs.</summary>
public sealed class OptionsPatientSupportContentProvider(IOptions<PatientSupportOptions> options)
    : IPatientSupportContentProvider
{
    private readonly IOptions<PatientSupportOptions> _options = options;

    public Task<PatientSupportContentDto> GetContentAsync(CancellationToken cancellationToken = default)
    {
        var o = _options.Value;
        var dto = new PatientSupportContentDto
        {
            SupportEmail = o.SupportEmail,
            SupportPhoneE164 = o.SupportPhoneE164,
            SupportPhoneDisplay = o.SupportPhoneDisplay,
            Faq = o.Faq
                .Where(f => !string.IsNullOrWhiteSpace(f.Question))
                .Select(f => new PatientSupportFaqItemDto
                {
                    Question = f.Question.Trim(),
                    Answer = f.Answer.Trim()
                })
                .ToArray()
        };
        return Task.FromResult(dto);
    }
}
