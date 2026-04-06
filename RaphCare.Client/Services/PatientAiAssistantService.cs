using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.AiAssistant;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>HTTP access to <c>api/patient/ai-assistant</c> until operations exist on <see cref="IClient"/> (NSwag regen).</summary>
public sealed class PatientAiAssistantService(IClient client, HttpClient httpClient) : BaseHttpService(client, httpClient), IPatientAiAssistantService
{
    private const string ChatUri = "api/patient/ai-assistant/chat";

    public Task<Response<PatientAssistantReplyViewModel>> SendMessageAsync(
        SendMyPatientAssistantMessageRequest request,
        CancellationToken cancellationToken = default) =>
        PostAsync<PatientAssistantReplyViewModel>(ChatUri, request, cancellationToken);
}
