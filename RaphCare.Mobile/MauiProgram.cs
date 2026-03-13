using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RaphCare.Mobile.Features.Auth.Views;
using RaphCare.Mobile.Features.Home.Views;
using RaphCare.Client;
using RaphCare.Mobile.Core.Features.Auth.Services;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;

namespace RaphCare.Mobile
{
    public static class MauiProgram
    {
        /// <summary>
        /// Service provider for resolving dependencies when Shell creates pages via DataTemplate (parameterless constructor).
        /// </summary>
        public static IServiceProvider? ServiceProvider { get; private set; }

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // Configuration (appsettings.json)
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Entra auth (Shared/Services/Auth) from configuration
            builder.Services.Configure<Core.Shared.Services.Auth.EntraAuthOptions>(
                builder.Configuration.GetSection(Core.Shared.Services.Auth.EntraAuthOptions.SectionName));
            builder.Services.AddSingleton<Core.Shared.Services.Auth.IAuthService, Core.Shared.Services.Auth.EntraAuthService>();
            builder.Services.AddSingleton<RaphCare.Client.Contracts.IAccessTokenProvider, SecureStorageAccessTokenProvider>();

            builder.Services.AddTransient<LandingViewModel>();
            builder.Services.AddTransient<RegisterOptionsViewModel>();
            builder.Services.AddTransient<RegisterEmailViewModel>();
            builder.Services.AddTransient<VerifyEmailViewModel>();
            builder.Services.AddTransient<SignInViewModel>();

            builder.Services.AddTransient<LandingPage>();
            builder.Services.AddTransient<RegisterOptionsPage>();
            builder.Services.AddTransient<RegisterEmailPage>();
            builder.Services.AddTransient<VerifyEmailPage>();
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<RaphCare.Mobile.Features.Records.Views.RecordsPage>();
            builder.Services.AddTransient<RaphCare.Mobile.Features.Appointments.Views.AppointmentsPage>();
            builder.Services.AddTransient<RaphCare.Mobile.Features.Insurance.Views.InsurancePage>();
            builder.Services.AddTransient<RaphCare.Mobile.Features.Settings.Views.SettingsPage>();
            builder.Services.AddTransient<RaphCare.Mobile.Shared.Views.UnderConstructionPage>();
            builder.Services.AddTransient<AppShell>();

            // API client with bearer token (base URL from config, with fallback)
            var apiBaseAddress = builder.Configuration["Api:BaseAddress"] ?? "https://localhost:7001/";
            builder.Services.AddRaphCareClient(client =>
            {
                client.BaseAddress = new Uri(apiBaseAddress);
            }, useBearerToken: true);

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            ServiceProvider = app.Services;
            return app;
        }
    }
}
