using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Onboarding.Commands.CreatePatientFromVoice;

/// <summary>
/// Creates a patient from voice-recorded onboarding: transcribes audio, extracts data, creates patient, stores recording metadata.
/// </summary>
public class CreatePatientFromVoiceCommand : IRequest<CreatePatientFromVoiceResult>, IAllowAnonymousRequest
{
    /// <summary>Audio stream (e.g. from uploaded file).</summary>
    public Stream AudioStream { get; set; } = null!;

    /// <summary>Language code for transcription (e.g. en-ZA).</summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>OTP-verified phone number for the registering user.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Clinic to register the patient under.</summary>
    public Guid ClinicId { get; set; }

    /// <summary>Audio content type from the upload (for example audio/wav).</summary>
    public string ContentType { get; set; } = "audio/wav";
}

/// <summary>
/// Result of voice-based patient creation.
/// </summary>
public class CreatePatientFromVoiceResult
{
    public Guid PatientId { get; set; }
    public string Transcription { get; set; } = string.Empty;
}
