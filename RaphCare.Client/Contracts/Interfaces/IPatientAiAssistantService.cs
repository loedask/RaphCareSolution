using RaphCare.Client.Contracts;
using RaphCare.Client.Models.AiAssistant;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient AI assistant chat (safe placeholder or future LLM behind the API).</summary>
public interface IPatientAiAssistantService
{
    Task<Response<PatientAssistantReplyViewModel>> SendMessageAsync(
        SendMyPatientAssistantMessageRequest request,
        CancellationToken cancellationToken = default);
}
