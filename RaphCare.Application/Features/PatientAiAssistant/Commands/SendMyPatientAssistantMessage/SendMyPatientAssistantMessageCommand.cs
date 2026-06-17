using MediatR;
using RaphCare.Application.Features.PatientAiAssistant.DTOs;

namespace RaphCare.Application.Features.PatientAiAssistant.Commands.SendMyPatientAssistantMessage;

/// <summary>Patient-typed message only; no clinical record context is attached in this vertical.</summary>
public sealed class SendMyPatientAssistantMessageCommand : IRequest<PatientAssistantReplyDto>
{
    public string Message { get; set; } = string.Empty;
}
