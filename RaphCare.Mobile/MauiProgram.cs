using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Mobile.Core.Infrastructure.Composition;
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
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
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
