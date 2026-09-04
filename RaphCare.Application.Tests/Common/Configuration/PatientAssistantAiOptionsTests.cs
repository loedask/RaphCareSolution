using RaphCare.Application.Common.Configuration;
using Xunit;

namespace RaphCare.Application.Tests.Common.Configuration;

public sealed class PatientAssistantAiOptionsTests
{
    [Fact]
    public void IsAzureOpenAiConfiguredWhenEndpointAndKeySetUsesDefaultGpt41MiniDeployment()
    {
        var options = new PatientAssistantAiOptions
        {
            AzureOpenAiEndpoint = "https://raphcare-openai.openai.azure.com/",
            AzureOpenAiApiKey = "test-key"
        };

        Assert.Equal("gpt-4.1-mini", options.AzureOpenAiDeployment);
        Assert.True(options.IsAzureOpenAiConfigured);
    }

    [Fact]
    public void IsAzureOpenAiConfiguredWhenEndpointMissingIsFalseEvenWithDefaultDeployment()
    {
        var options = new PatientAssistantAiOptions
        {
            AzureOpenAiApiKey = "test-key"
        };

        Assert.False(options.IsAzureOpenAiConfigured);
    }
}
