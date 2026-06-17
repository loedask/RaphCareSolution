using Microsoft.Maui.Media;

namespace RaphCare.Mobile.Core.Features.Auth.Services;

/// <summary>Speaks voice-registration prompts using the device text-to-speech engine.</summary>
internal static class VoiceRegistrationPromptSpeaker
{
    public static async Task SpeakAsync(string text, string? languageTag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var locales = await TextToSpeech.Default.GetLocalesAsync().ConfigureAwait(false);
        var options = new SpeechOptions
        {
            Pitch = 1f,
            Volume = 1f,
            Locale = ResolveLocale(locales, languageTag),
        };

        await TextToSpeech.Default.SpeakAsync(text, options, cancellationToken).ConfigureAwait(false);
    }

    public static Task StopAsync()
    {
        // Cancellation on SpeakAsync is the supported stop path; no separate ITextToSpeech stop API.
        return Task.CompletedTask;
    }

    private static Locale? ResolveLocale(IEnumerable<Locale> locales, string? languageTag)
    {
        var list = locales.ToList();
        if (list.Count == 0)
            return null;

        if (string.IsNullOrWhiteSpace(languageTag))
            return list.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase)) ?? list[0];

        var normalized = languageTag.Trim().Replace('_', '-');
        var exact = list.FirstOrDefault(l =>
            string.Equals(l.Language, normalized, StringComparison.OrdinalIgnoreCase)
            || string.Equals(l.Name, normalized, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
            return exact;

        var primary = normalized.Split('-')[0];
        return list.FirstOrDefault(l =>
                   l.Language.StartsWith(primary, StringComparison.OrdinalIgnoreCase)
                   || l.Name.StartsWith(primary, StringComparison.OrdinalIgnoreCase))
               ?? list[0];
    }
}
