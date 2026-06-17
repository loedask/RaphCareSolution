using FluentValidation;
using RaphCare.Application.Features.PatientDevices.DTOs;

namespace RaphCare.Application.Features.PatientDevices.Commands.SyncMyDeviceReadings;

public sealed class SyncMyDeviceReadingsValidator : AbstractValidator<SyncMyDeviceReadingsCommand>
{
    private const int MaxBatchTotal = 500;

    public SyncMyDeviceReadingsValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();

        RuleFor(x => x)
            .Must(c => (c.HeartRates?.Count ?? 0) + (c.Spo2?.Count ?? 0) > 0)
            .WithMessage("Provide at least one heart rate or SpO2 reading.")
            .Must(c => (c.HeartRates?.Count ?? 0) + (c.Spo2?.Count ?? 0) <= MaxBatchTotal)
            .WithMessage($"A maximum of {MaxBatchTotal} readings per request is allowed.");

        RuleForEach(x => x.HeartRates).ChildRules(hr =>
        {
            hr.RuleFor(p => p.BeatsPerMinute).InclusiveBetween(30, 220);
        });

        RuleForEach(x => x.Spo2).ChildRules(sp =>
        {
            sp.RuleFor(p => p.SpO2).InclusiveBetween(50, 100);
            sp.RuleFor(p => p.PulseRate!.Value).InclusiveBetween(30, 220)
                .When(p => p.PulseRate.HasValue);
        });
    }
}
