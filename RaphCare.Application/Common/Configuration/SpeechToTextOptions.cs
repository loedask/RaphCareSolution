namespace RaphCare.Application.Common.Configuration;

/// <summary>Speech-to-text provider selection for voice onboarding. Bound from <see cref="SectionName"/>.</summary>
public class SpeechToTextOptions
{
    public const string SectionName = "SpeechToText";

    /// <summary>Active STT backend. Defaults to <see cref="SpeechToTextProvider.Whisper"/>.</summary>
    public SpeechToTextProvider Provider { get; set; } = SpeechToTextProvider.Whisper;

    public WhisperSpeechToTextOptions Whisper { get; set; } = new();

    public AzureSpeechToTextOptions Azure { get; set; } = new();
}

public enum SpeechToTextProvider
{
    Whisper = 0,
    Azure = 1,
}

/// <summary>Local Whisper.net (ggml) model settings.</summary>
public class WhisperSpeechToTextOptions
{
    /// <summary>Absolute path to a ggml model file (e.g. ggml-base.bin). When empty, uses <see cref="ModelDirectory"/> + model file name.</summary>
    public string? ModelPath { get; set; }

    /// <summary>Directory for model files when <see cref="ModelPath"/> is not set. Default: %LocalApplicationData%/RaphCare/whisper.</summary>
    public string? ModelDirectory { get; set; }

    /// <summary>Ggml model size to download when missing: Tiny, Base, Small, Medium, Large, etc.</summary>
    public string ModelType { get; set; } = "Base";

    /// <summary>When true and the model file is missing, download it from Hugging Face on first use.</summary>
    public bool AutoDownloadModel { get; set; } = true;
}

/// <summary>Azure Cognitive Services Speech settings.</summary>
public class AzureSpeechToTextOptions
{
    public string? SubscriptionKey { get; set; }

    public string? Region { get; set; }

    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(SubscriptionKey)
        && !string.IsNullOrWhiteSpace(Region);
}
