using Microsoft.Extensions.Logging;
using RaphCare.Mobile.Features.Auth.Services;
using RaphCare.Mobile.Features.Auth.ViewModels;
using RaphCare.Mobile.Features.Auth.Views;
using RaphCare.Mobile.Features.Home.Views;
using RaphCare.Client;

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
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Entra auth (Shared/Services/Auth)
            builder.Services.Configure<Shared.Services.Auth.EntraAuthOptions>(options =>
            {
                options.ClientId = "YOUR_CLIENT_ID"; // Replace with app registration client ID
                options.TenantId = "common";
                options.ApiScope = "api://raphcare-api/.default";
            });
            builder.Services.AddSingleton<Shared.Services.Auth.IAuthService, Shared.Services.Auth.EntraAuthService>();
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

            // API client with bearer token (base URL should come from config)
            builder.Services.AddRaphCareClient(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7001/"); // Replace with your API base URL
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
