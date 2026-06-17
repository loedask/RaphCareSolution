using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.AiAssistant;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> patient AI assistant chat.</summary>
public sealed class PatientAiAssistantService(IClient client) : IPatientAiAssistantService
{
    private readonly IClient _client = client;

    public async Task<Response<PatientAssistantReplyViewModel>> SendMessageAsync(
        SendMyPatientAssistantMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new SendMyPatientAssistantMessageCommand
            {
                Message = request.Message,
            };
            var dto = await _client.SendMyPatientAssistantMessageAsync(body, cancellationToken).ConfigureAwait(false);
            return Response<PatientAssistantReplyViewModel>.Success(Map(dto));
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PatientAssistantReplyViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static PatientAssistantReplyViewModel Map(PatientAssistantReplyDto d) =>
        new()
        {
            Reply = d.Reply ?? string.Empty,
            MedicalDisclaimer = d.MedicalDisclaimer ?? string.Empty,
        };
}
