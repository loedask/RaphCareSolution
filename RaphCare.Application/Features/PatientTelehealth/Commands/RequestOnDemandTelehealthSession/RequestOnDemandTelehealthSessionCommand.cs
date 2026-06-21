using MediatR;

namespace RaphCare.Application.Features.PatientTelehealth.Commands.RequestOnDemandTelehealthSession;

/// <summary>Creates an immediate Agora telehealth session (appointment + visit + tele session) for the signed-in patient.</summary>
public sealed class RequestOnDemandTelehealthSessionCommand : IRequest<Guid>
{
    public Guid? ClinicId { get; set; }
    public Guid? ProviderId { get; set; }

    /// <summary>video, audio, or lowband (stored on the appointment reason for now).</summary>
    public string? CallMode { get; set; }
}
