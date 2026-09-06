using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Infrastructure.Services;
using Xunit;

namespace RaphCare.Infrastructure.Tests.Services;

public sealed class AIServiceTests
{
    [Fact]
    public async Task GeneratePatientAssistantReplyAsyncWhenAzureOpenAiNotConfiguredReturnsPlaceholderAndDoesNotCallHttp()
    {
        var handler = new CapturingHandler();
        var options = new PatientAssistantAiOptions
        {
            AzureOpenAiEndpoint = "",
            AzureOpenAiApiKey = "",
            PlaceholderReply = "Thanks for your message. A full AI model is not connected."
        };
        var service = CreateService(options, handler);

        var reply = await service.GeneratePatientAssistantReplyAsync("I have a headache", cancellationToken: CancellationToken.None);

        Assert.Equal(options.PlaceholderReply, reply);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task GenerateSummaryAsyncWhenAzureOpenAiNotConfiguredReturnsUnavailableMessageAndDoesNotCallHttp()
    {
        var handler = new CapturingHandler();
        var service = CreateService(new PatientAssistantAiOptions(), handler);

        var reply = await service.GenerateSummaryAsync("Admission reason: malaria. Vitals: HR 88.", CancellationToken.None);

        Assert.Contains("AI drafting is not connected", reply, StringComparison.Ordinal);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task GeneratePatientAssistantReplyAsyncWhenConfiguredPostsToGpt41MiniAndReturnsAssistantContent()
    {
        var handler = new CapturingHandler
        {
            Response = JsonChatResponse("Drink water and rest. Contact your clinician if it worsens.")
        };
        var options = ConfiguredOptions();
        var service = CreateService(options, handler);

        var reply = await service.GeneratePatientAssistantReplyAsync("I feel tired", cancellationToken: CancellationToken.None);

        Assert.Equal("Drink water and rest. Contact your clinician if it worsens.", reply);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal(
            "https://raphcare-openai.openai.azure.com/openai/deployments/gpt-4.1-mini/chat/completions?api-version=2024-08-01-preview",
            handler.LastRequest.RequestUri!.ToString());
        Assert.True(handler.LastRequest.Headers.TryGetValues("api-key", out var keys));
        Assert.Equal("test-key", Assert.Single(keys!));
        Assert.Contains("\"max_tokens\":512", handler.LastBody, StringComparison.Ordinal);
        Assert.DoesNotContain("max_completion_tokens", handler.LastBody, StringComparison.Ordinal);
        Assert.Contains("I feel tired", handler.LastBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateSummaryAsyncWhenChatFailsReturnsUnavailableMessage()
    {
        var handler = new CapturingHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("""{"error":{"code":"RateLimitReached","message":"retry"}}""", Encoding.UTF8, "application/json")
            }
        };
        var service = CreateService(ConfiguredOptions(), handler);

        var reply = await service.GenerateSummaryAsync("Admission reason: malaria.", CancellationToken.None);

        Assert.Contains("AI drafting is not connected", reply, StringComparison.Ordinal);
        Assert.NotNull(handler.LastRequest);
    }

    [Fact]
    public async Task GeneratePatientAssistantReplyAsyncWhenHttpFailsReturnsPlaceholderNotAzureErrorBody()
    {
        var handler = new CapturingHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"error":{"code":"DeploymentNotFound","message":"secret internals"}}""", Encoding.UTF8, "application/json")
            }
        };
        var options = ConfiguredOptions();
        var service = CreateService(options, handler);

        var reply = await service.GeneratePatientAssistantReplyAsync("Hello", cancellationToken: CancellationToken.None);

        Assert.Equal(options.PlaceholderReply, reply);
        Assert.DoesNotContain("secret internals", reply, StringComparison.Ordinal);
        Assert.DoesNotContain("DeploymentNotFound", reply, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GeneratePatientAssistantReplyAsyncIncludesPriorTurnsInRequestBody()
    {
        var handler = new CapturingHandler
        {
            Response = JsonChatResponse("Based on what you said earlier, try a short walk.")
        };
        var service = CreateService(ConfiguredOptions(), handler);

        var reply = await service.GeneratePatientAssistantReplyAsync(
            "What else can I try?",
            [
                ("assistant", "Hello! How can I help?"),
                ("user", "I am not sleeping well."),
            ],
            CancellationToken.None);

        Assert.Equal("Based on what you said earlier, try a short walk.", reply);
        Assert.Contains("I am not sleeping well.", handler.LastBody, StringComparison.Ordinal);
        Assert.Contains("What else can I try?", handler.LastBody, StringComparison.Ordinal);
        Assert.Contains("\"role\":\"assistant\"", handler.LastBody, StringComparison.Ordinal);
        Assert.Contains("\"role\":\"user\"", handler.LastBody, StringComparison.Ordinal);
    }

    private static PatientAssistantAiOptions ConfiguredOptions() => new()
    {
        AzureOpenAiEndpoint = "https://raphcare-openai.openai.azure.com/",
        AzureOpenAiApiKey = "test-key",
        AzureOpenAiDeployment = PatientAssistantAiOptions.DefaultDeploymentName,
        AzureOpenAiApiVersion = PatientAssistantAiOptions.DefaultApiVersion,
        MaxCompletionTokens = 512,
        PlaceholderReply = "Thanks for your message. A full AI model is not connected."
    };

    private static AIService CreateService(PatientAssistantAiOptions options, HttpMessageHandler handler) =>
        new(
            NullLogger<AIService>.Instance,
            new StaticOptionsMonitor<PatientAssistantAiOptions>(options),
            new StubHttpClientFactory(handler));

    private static HttpResponseMessage JsonChatResponse(string content)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new { message = new { role = "assistant", content } }
            }
        });
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        public string? LastBody { get; private set; }

        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
                LastBody = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return Response;
        }
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class StaticOptionsMonitor<T>(T current) : IOptionsMonitor<T>
    {
        public T CurrentValue => current;

        public T Get(string? name) => current;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
