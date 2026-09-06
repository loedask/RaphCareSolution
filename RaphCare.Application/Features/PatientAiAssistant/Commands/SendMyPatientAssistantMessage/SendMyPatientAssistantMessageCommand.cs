using MediatR;
using RaphCare.Application.Features.PatientAiAssistant.DTOs;

namespace RaphCare.Application.Features.PatientAiAssistant.Commands.SendMyPatientAssistantMessage;

/// <summary>Patient-typed message plus optional short prior chat window; no clinical record context.</summary>
public sealed class SendMyPatientAssistantMessageCommand : IRequest<PatientAssistantReplyDto>
{
    public string Message { get; set; } = string.Empty;

    /// <summary>Earlier turns from the phone session (server windows again for cost).</summary>
    public List<PatientAssistantPriorMessageDto>? PriorMessages { get; set; }
}
