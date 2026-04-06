namespace RaphCare.Application.Common.Interfaces;

public interface IAIService
{
    Task<string> GenerateSummaryAsync(string input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Produces a reply for the signed-in patient's assistant chat. Implementations must not silently inject PHI;
    /// callers pass only the patient's current message text.
    /// </summary>
    Task<string> GeneratePatientAssistantReplyAsync(string userMessage, CancellationToken cancellationToken = default);
}

