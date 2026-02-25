namespace RaphCare.Application.Common.Interfaces;

public interface IAIService
{
    Task<string> GenerateSummaryAsync(string input, CancellationToken cancellationToken = default);
}

