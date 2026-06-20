using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>AI integration: patient assistant uses Azure OpenAI when configured; other entry points remain placeholders.</summary>
public sealed partial class AIService : IAIService
{
    private const int MaxUserMessageLength = 8000;

    private readonly ILogger<AIService> _logger;
    private readonly IOptionsMonitor<PatientAssistantAiOptions> _patientAssistantOptions;
    private readonly IHttpClientFactory _httpClientFactory;

    public AIService(
        ILogger<AIService> logger,
        IOptionsMonitor<PatientAssistantAiOptions> patientAssistantOptions,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _patientAssistantOptions = patientAssistantOptions;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public Task<string> GenerateSummaryAsync(string input, CancellationToken cancellationToken = default)
    {
        LogGenerateSummaryPlaceholder(input?.Length ?? 0);
        return Task.FromResult("[Placeholder summary]");
    }

    /// <inheritdoc />
    public async Task<string> GeneratePatientAssistantReplyAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var opts = _patientAssistantOptions.CurrentValue;
        var trimmed = userMessage.Trim();
        if (trimmed.Length > MaxUserMessageLength)
            trimmed = trimmed[..MaxUserMessageLength];

        if (!opts.IsAzureOpenAiConfigured)
        {
            LogPatientAssistantPlaceholder(trimmed.Length);
            return opts.PlaceholderReply;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Remove("api-key");
            client.DefaultRequestHeaders.TryAddWithoutValidation("api-key", opts.AzureOpenAiApiKey);

            var endpoint = opts.AzureOpenAiEndpoint!.TrimEnd('/');
            var deployment = Uri.EscapeDataString(opts.AzureOpenAiDeployment!);
            var apiVersion = Uri.EscapeDataString(opts.AzureOpenAiApiVersion);
            var url =
                $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

            var systemPrompt = string.IsNullOrWhiteSpace(opts.SystemPrompt)
                ? DefaultAssistantSystemPrompt
                : opts.SystemPrompt.Trim();

            var requestBody = new AzureOpenAiChatRequest
            {
                Messages =
                [
                    new AzureOpenAiChatMessage { Role = "system", Content = systemPrompt },
                    new AzureOpenAiChatMessage { Role = "user", Content = trimmed },
                ],
                MaxCompletionTokens = Math.Clamp(opts.MaxCompletionTokens, 64, 4096),
                Temperature = Math.Clamp(opts.Temperature, 0, 2),
            };

            var json = JsonSerializer.Serialize(requestBody, AzureJsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(new Uri(url), content, cancellationToken).ConfigureAwait(false);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                LogAzureOpenAiChatFailed((int)response.StatusCode, responseText.Length);
                return opts.PlaceholderReply;
            }

            using var doc = JsonDocument.Parse(responseText);
            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(reply))
            {
                LogAzureOpenAiEmptyReply();
                return opts.PlaceholderReply;
            }

            return reply.Trim();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogAzureOpenAiCallFailed(ex);
            return _patientAssistantOptions.CurrentValue.PlaceholderReply;
        }
    }

    /// <inheritdoc />
    public Task<string> GenerateWellnessInsightAsync(string patientContext, CancellationToken cancellationToken = default)
    {
        LogGenerateWellnessInsightPlaceholder();
        return Task.FromResult("[Placeholder wellness insight]");
    }

    /// <inheritdoc />
    public Task<decimal> CalculateRiskScoreAsync(string clinicalData, CancellationToken cancellationToken = default)
    {
        LogCalculateRiskScorePlaceholder();
        return Task.FromResult(0m);
    }

    private static string DefaultAssistantSystemPrompt =>
        """
        You are RaphCare's patient wellness assistant. Reply in plain language, briefly and supportively.
        Never diagnose medical conditions, prescribe treatments, or give emergency instructions.
        For urgent symptoms or emergencies, tell the user to contact local emergency services or their clinician immediately.
        For clinical questions, encourage following their care team's advice.
        """;

    private static readonly JsonSerializerOptions AzureJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private sealed class AzureOpenAiChatRequest
    {
        public List<AzureOpenAiChatMessage> Messages { get; set; } = [];

        public int MaxCompletionTokens { get; set; }

        public double Temperature { get; set; }
    }

    private sealed class AzureOpenAiChatMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "AI GenerateSummary placeholder: Input length={Length}")]
    private partial void LogGenerateSummaryPlaceholder(int length);

    [LoggerMessage(Level = LogLevel.Information, Message = "AI patient assistant (placeholder): message length {Length}")]
    private partial void LogPatientAssistantPlaceholder(int length);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Azure OpenAI chat failed: {Status} body length {Length}")]
    private partial void LogAzureOpenAiChatFailed(int status, int length);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Azure OpenAI returned an empty assistant message.")]
    private partial void LogAzureOpenAiEmptyReply();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Azure OpenAI patient assistant call failed.")]
    private partial void LogAzureOpenAiCallFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "AI GenerateWellnessInsight placeholder")]
    private partial void LogGenerateWellnessInsightPlaceholder();

    [LoggerMessage(Level = LogLevel.Information, Message = "AI CalculateRiskScore placeholder")]
    private partial void LogCalculateRiskScorePlaceholder();
}
