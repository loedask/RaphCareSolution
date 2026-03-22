using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plugin.Maui.Audio;
using RaphCare.Mobile.Core.Infrastructure.Composition;
#if IOS || MACCATALYST
using AVFoundation;
#endif
using RaphCare.Mobile.Core.Infrastructure.DependencyInjection;
using RaphCare.Mobile.Core.Shared.Services.FeatureFlags;

namespace RaphCare.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

#if DEBUG
        builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
#endif

        builder.Configuration.AddUserSecrets(typeof(App).Assembly, optional: true);

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Concept: display = Space Grotesk, body = DM Sans (google/fonts OFL variable TTFs under Resources/Fonts).
                fonts.AddFont("SpaceGrotesk-VariableFont_wght.ttf", "SpaceGrotesk");
                fonts.AddFont("DMSans-VariableFont_opsz_wght.ttf", "DMSans");
                fonts.AddFont("DMSans-Italic-VariableFont_opsz_wght.ttf", "DMSansItalic");
            })
            .AddAudio(configureRecordingOptions: static ro =>
            {
#if IOS || MACCATALYST
                ro.Category = AVAudioSessionCategory.PlayAndRecord;
                ro.Mode = AVAudioSessionMode.Default;
                ro.CategoryOptions = AVAudioSessionCategoryOptions.DefaultToSpeaker;
#endif
            });

        builder.Services.AddRaphCareMobile(builder.Configuration);

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        MobileServiceHub.SetRootProvider(app.Services);

        var featureOptions = app.Services.GetRequiredService<IOptions<FeatureFlagOptions>>().Value;
        FeatureFlags.Initialize(featureOptions);

        return app;
    }
}
