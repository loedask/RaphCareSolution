namespace RaphCare.Infrastructure.Configuration;

/// <summary>Azure Blob (or local Development fallback) settings for private patient files.</summary>
public sealed class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    public string ConnectionString { get; set; } = string.Empty;

    public string PhotosContainer { get; set; } = "patient-photos";

    public string VoiceRecordingsContainer { get; set; } = "voice-recordings";
}
