namespace RaphCare.Application.Common.Configuration;

public sealed class RaphCarePortalOptions
{
    public const string SectionName = "RaphCare";

    public string WebPortalBaseUrl { get; set; } = "https://localhost:7092";
}
