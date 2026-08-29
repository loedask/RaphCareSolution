using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plugin.Maui.Audio;
using RaphCare.Mobile.Core.Infrastructure.Composition;
#if IOS || MACCATALYST
using AVFoundation;
#endif
using RaphCare.Mobile.Core.Features.CareTelehealth.Views;
using RaphCare.Mobile.Core.Infrastructure.DependencyInjection;
using RaphCare.Mobile.Core.Common.Services.FeatureFlags;
#if ANDROID
using RaphCare.Mobile.Platforms.Android.Telehealth;
#elif IOS
using RaphCare.Mobile.Platforms.iOS.Telehealth;
#elif MACCATALYST
using RaphCare.Mobile.Platforms.MacCatalyst.Telehealth;
#elif WINDOWS
using RaphCare.Mobile.Platforms.Windows.Telehealth;
#endif

namespace RaphCare.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        AddAppPackageJson(builder.Configuration, "appsettings.json");

#if DEBUG
        AddAppPackageJson(builder.Configuration, "appsettings.Development.json");
#else
        // Release / Play / sideload: hosted API URL (publish scripts may rewrite this file before packaging).
        AddAppPackageJson(builder.Configuration, "appsettings.TestHosting.json");
#endif

        builder.Configuration.AddUserSecrets(typeof(App).Assembly, optional: true);

        builder
            .UseMauiApp<App>()
            .ConfigureMauiHandlers(static handlers =>
            {
#if ANDROID
                handlers.AddHandler<TelehealthVideoView, TelehealthVideoViewHandler>();
#elif IOS
                handlers.AddHandler<TelehealthVideoView, TelehealthVideoViewHandler>();
#elif MACCATALYST
                handlers.AddHandler<TelehealthVideoView, TelehealthVideoViewHandler>();
#elif WINDOWS
                handlers.AddHandler<TelehealthVideoView, TelehealthVideoViewHandler>();
#endif
            })
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

    /// <summary>
    /// Loads JSON from the app package (Android/iOS assets). Plain <c>AddJsonFile</c> only sees the
    /// desktop output folder, so Release APKs would keep falling back to localhost.
    /// </summary>
    private static void AddAppPackageJson(ConfigurationManager configuration, string fileName)
    {
        try
        {
            using var package = FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
            var copy = new MemoryStream();
            package.CopyTo(copy);
            copy.Position = 0;
            configuration.AddJsonStream(copy);
        }
        catch (FileNotFoundException)
        {
            // optional files (Development / TestHosting) may be absent in some hosts
        }
        catch (DirectoryNotFoundException)
        {
            // same
        }
    }
}
