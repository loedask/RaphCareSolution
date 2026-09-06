using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>AI integration: Azure OpenAI when configured; safe placeholders otherwise.</summary>
public sealed partial class AIService : IAIService
{
    /// <summary>Named <see cref="IHttpClientFactory"/> client used for Azure OpenAI chat completions.</summary>
    public const string HttpClientName = "AzureOpenAi";

    private const int MaxUserMessageLength = 8000;
    private const int MaxClinicalDraftLength = 4000;

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
    public async Task<string> GenerateSummaryAsync(string input, CancellationToken cancellationToken = default)
    {
        var trimmed = (input ?? string.Empty).Trim();
        if (trimmed.Length > MaxUserMessageLength)
            trimmed = trimmed[..MaxUserMessageLength];

        var opts = _patientAssistantOptions.CurrentValue;
        if (!opts.IsAzureOpenAiConfigured)
        {
            LogGenerateSummaryPlaceholder(trimmed.Length);
            return ClinicalDraftUnavailableMessage;
        }

        var reply = await CompleteChatAsync(
                ClinicalDraftSystemPrompt,
                trimmed,
                priorTurns: null,
                maxTokens: Math.Clamp(opts.MaxCompletionTokens, 64, 2048),
                temperature: 0.3,
                cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(reply))
            return ClinicalDraftUnavailableMessage;

        reply = reply.Trim();
        return reply.Length <= MaxClinicalDraftLength ? reply : reply[..MaxClinicalDraftLength];
    }

    /// <inheritdoc />
    public async Task<string> GeneratePatientAssistantReplyAsync(
        string userMessage,
        IReadOnlyList<(string Role, string Content)>? priorTurns = null,
        CancellationToken cancellationToken = default)
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

        var systemPrompt = string.IsNullOrWhiteSpace(opts.SystemPrompt)
            ? DefaultAssistantSystemPrompt
            : opts.SystemPrompt.Trim();

        var reply = await CompleteChatAsync(
                systemPrompt,
                trimmed,
                priorTurns,
                maxTokens: Math.Clamp(opts.MaxCompletionTokens, 64, 4096),
                temperature: Math.Clamp(opts.Temperature, 0, 2),
                cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(reply) ? opts.PlaceholderReply : reply.Trim();
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

    private async Task<string?> CompleteChatAsync(
        string systemPrompt,
        string userContent,
        IReadOnlyList<(string Role, string Content)>? priorTurns,
        int maxTokens,
        double temperature,
        CancellationToken cancellationToken)
    {
        var opts = _patientAssistantOptions.CurrentValue;
        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);

            var endpoint = opts.AzureOpenAiEndpoint!.TrimEnd('/');
            var deployment = Uri.EscapeDataString(opts.AzureOpenAiDeployment!);
            var apiVersion = Uri.EscapeDataString(opts.AzureOpenAiApiVersion);
            var url =
                $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

            var messages = new List<AzureOpenAiChatMessage>
            {
                new() { Role = "system", Content = systemPrompt },
            };

            if (priorTurns is { Count: > 0 })
            {
                foreach (var (role, content) in priorTurns)
                {
                    if (string.IsNullOrWhiteSpace(content))
                        continue;
                    if (role is not ("user" or "assistant"))
                        continue;
                    messages.Add(new AzureOpenAiChatMessage { Role = role, Content = content });
                }
            }

            messages.Add(new AzureOpenAiChatMessage { Role = "user", Content = userContent });

            var requestBody = new AzureOpenAiChatRequest
            {
                Messages = messages,
                MaxTokens = maxTokens,
                Temperature = temperature,
            };

            var json = JsonSerializer.Serialize(requestBody, AzureJsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("api-key", opts.AzureOpenAiApiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                LogAzureOpenAiChatFailed((int)response.StatusCode, TryGetAzureErrorCode(responseText), responseText.Length);
                return null;
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
                return null;
            }

            return reply;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogAzureOpenAiCallFailed(ex);
            return null;
        }
    }

    private static string ClinicalDraftUnavailableMessage =>
        "AI drafting is not connected on this server yet. Write the discharge summary from the ward notes and stay reason.";

    private static string ClinicalDraftSystemPrompt =>
        """
        You draft short hospital discharge summaries for clinicians. Write in plain clinical English.
        The input is intentionally limited: admission reason and ward vitals only (no free-text ward notes).
        Use only those facts. Do not invent diagnoses, medicines, or follow-up plans.
        Keep it under 250 words. Use short paragraphs. Do not address the patient directly.
        This is a staff draft only; a clinician will edit it before saving.
        """;

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

    private static string? TryGetAzureErrorCode(string responseText)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseText);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("code", out var code))
            {
                return code.GetString();
            }
        }
        catch (JsonException)
        {
            // Response is not JSON; log status and length only.
        }

        return null;
    }

    private sealed class AzureOpenAiChatRequest
    {
        public List<AzureOpenAiChatMessage> Messages { get; set; } = [];

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

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

    [LoggerMessage(Level = LogLevel.Warning, Message = "Azure OpenAI chat failed: {Status} code {ErrorCode} body length {Length}")]
    private partial void LogAzureOpenAiChatFailed(int status, string? errorCode, int length);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Azure OpenAI returned an empty assistant message.")]
    private partial void LogAzureOpenAiEmptyReply();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Azure OpenAI patient assistant call failed.")]
    private partial void LogAzureOpenAiCallFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "AI GenerateWellnessInsight placeholder")]
    private partial void LogGenerateWellnessInsightPlaceholder();

    [LoggerMessage(Level = LogLevel.Information, Message = "AI CalculateRiskScore placeholder")]
    private partial void LogCalculateRiskScorePlaceholder();
}
