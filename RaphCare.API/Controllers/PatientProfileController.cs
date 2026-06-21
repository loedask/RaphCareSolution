using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientProfile.Commands.UpdateMyPatientProfile;
using RaphCare.Application.Features.PatientProfile.Commands.UploadMyPatientProfilePhoto;
using RaphCare.Application.Features.PatientProfile.DTOs;
using RaphCare.Application.Features.PatientProfile.Queries.GetMyPatientProfile;
using RaphCare.Application.Features.PatientProfile.Queries.GetMyPatientProfilePhoto;

namespace RaphCare.API.Controllers;

/// <summary>Current patient demographics (<c>api/patient/profile</c>).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/profile")]
public sealed class PatientProfileController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet(Name = "GetMyPatientProfile")]
    [ProducesResponseType(typeof(MyPatientProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientProfileQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPut(Name = "UpdateMyPatientProfile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update([FromBody] UpdateMyPatientProfileCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("photo", Name = "UploadMyPatientProfilePhoto")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadMyPatientProfilePhotoResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadPhoto(IFormFile photo, CancellationToken cancellationToken)
    {
        if (photo is null || photo.Length == 0)
            return BadRequest(new { error = "Photo file is required." });

        await using var stream = photo.OpenReadStream();
        var photoUrl = await _mediator.Send(new UploadMyPatientProfilePhotoCommand
        {
            Content = stream,
            ContentType = photo.ContentType ?? "image/jpeg"
        }, cancellationToken).ConfigureAwait(false);

        return Ok(new UploadMyPatientProfilePhotoResult { PhotoUrl = photoUrl });
    }

    [HttpGet("photo", Name = "GetMyPatientProfilePhoto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPhoto(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientProfilePhotoQuery(), cancellationToken).ConfigureAwait(false);
        if (result is null)
            return NotFound();

        return File(result.Content, result.ContentType);
    }
}

public sealed class UploadMyPatientProfilePhotoResult
{
    public string PhotoUrl { get; set; } = string.Empty;
}
