using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientAiAssistant.DTOs;

namespace RaphCare.Application.Features.PatientAiAssistant.Commands.SendMyPatientAssistantMessage;

/// <summary>
/// Handles patient assistant chat: validates input, audits by patient id and length only (not message body),
/// and delegates text generation to <see cref="IAIService"/> (placeholder or future LLM with policy guardrails).
/// </summary>
public sealed class SendMyPatientAssistantMessageHandler(
    IAIService aiService,
    ICurrentUserService currentUser,
    IOptions<PatientAssistantAiOptions> options,
    ILogger<SendMyPatientAssistantMessageHandler> logger) : IRequestHandler<SendMyPatientAssistantMessageCommand, PatientAssistantReplyDto>
{
    private readonly IAIService _aiService = aiService;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IOptions<PatientAssistantAiOptions> _options = options;
    private readonly ILogger<SendMyPatientAssistantMessageHandler> _logger = logger;

    public async Task<PatientAssistantReplyDto> Handle(SendMyPatientAssistantMessageCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var message = NormalizeMessage(request.Message);
        _logger.LogInformation(
            "Patient assistant message accepted for patient {PatientId}; length {Length}.",
            patientId,
            message.Length);

        var reply = await _aiService.GeneratePatientAssistantReplyAsync(message, cancellationToken).ConfigureAwait(false);
        var opts = _options.Value;

        return new PatientAssistantReplyDto
        {
            Reply = reply,
            MedicalDisclaimer = opts.MedicalDisclaimer,
        };
    }

    private static string NormalizeMessage(string raw)
    {
        var trimmed = raw.Trim();
        var sb = new StringBuilder(trimmed.Length);
        foreach (var c in trimmed)
        {
            if (c is '\r' or '\n' or '\t' or >= ' ')
                sb.Append(c);
        }

        return sb.ToString();
    }
}
