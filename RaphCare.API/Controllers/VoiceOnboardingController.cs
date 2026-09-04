using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Onboarding.Commands.CreatePatientFromVoice;

namespace RaphCare.API.Controllers;

/// <summary>
/// Voice-first patient onboarding: accept recorded audio, transcribe, extract data, create patient, store recording reference.
/// Requires OTP-verified phone number (enforced by client/session).
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/onboarding")]
public class VoiceOnboardingController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a patient from voice recording. Consumes multipart form with audio file, language, phone number, and clinic.
    /// </summary>
    /// <param name="audioFile">Recorded voice file (e.g. voice.wav).</param>
    /// <param name="language">Language code (e.g. en-ZA).</param>
    /// <param name="phoneNumber">OTP-verified phone number (e.g. +27831234567).</param>
    /// <param name="clinicId">Clinic to register the patient under.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Patient ID and transcription text.</returns>
    [HttpPost("voice")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CreatePatientFromVoiceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFromVoice(
        IFormFile audioFile,
        [FromForm] string language,
        [FromForm] string phoneNumber,
        [FromForm] Guid clinicId,
        CancellationToken cancellationToken)
    {
        if (audioFile == null || audioFile.Length == 0)
            return BadRequest(new { error = "Audio file is required." });
        if (string.IsNullOrWhiteSpace(language))
            return BadRequest(new { error = "Language is required." });
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return BadRequest(new { error = "Phone number is required." });
        if (clinicId == Guid.Empty)
            return BadRequest(new { error = "Clinic ID is required." });

        await using var stream = audioFile.OpenReadStream();
        var command = new CreatePatientFromVoiceCommand
        {
            AudioStream = stream,
            Language = language.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            ClinicId = clinicId,
            ContentType = string.IsNullOrWhiteSpace(audioFile.ContentType) ? "audio/wav" : audioFile.ContentType
        };

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
