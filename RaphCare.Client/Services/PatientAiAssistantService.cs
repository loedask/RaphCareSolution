using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.AiAssistant;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientAiAssistantService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientAiAssistantService
{
    public async Task<Response<PatientAssistantReplyViewModel>> SendMessageAsync(
        SendMyPatientAssistantMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<AssistantReplyDto>("api/patient/ai-assistant/chat", request, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PatientAssistantReplyViewModel>.Failure(result.ErrorMessage ?? "Chat failed.", result.StatusCode);
        return Response<PatientAssistantReplyViewModel>.Success(Map(result.Data));
    }

    private static PatientAssistantReplyViewModel Map(AssistantReplyDto d) =>
        new()
        {
            Reply = d.Reply ?? string.Empty,
            MedicalDisclaimer = d.MedicalDisclaimer ?? string.Empty,
        };

    private sealed class AssistantReplyDto
    {
        public string? Reply { get; set; }
        public string? MedicalDisclaimer { get; set; }
    }
}
