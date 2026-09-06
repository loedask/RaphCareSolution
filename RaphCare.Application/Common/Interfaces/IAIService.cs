namespace RaphCare.Application.Common.Interfaces;

public interface IAIService
{
    Task<string> GenerateSummaryAsync(string input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Produces a reply for the signed-in patient's assistant chat. Implementations must not silently inject PHI;
    /// callers pass only the patient's typed turns (optional short prior window + current message).
    /// </summary>
    /// <param name="userMessage">Current user message.</param>
    /// <param name="priorTurns">Optional earlier <c>user</c>/<c>assistant</c> turns (already windowed by the caller).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<string> GeneratePatientAssistantReplyAsync(
        string userMessage,
        IReadOnlyList<(string Role, string Content)>? priorTurns = null,
        CancellationToken cancellationToken = default);
}

