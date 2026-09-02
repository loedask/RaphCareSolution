using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RaphCare.Client;
using RaphCare.Client.Contracts;
using RaphCare.Web;
using RaphCare.Web.Services;
using RaphCare.Web.Services.Localization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5281";
if (!apiBase.EndsWith('/'))
    apiBase += "/";

builder.Services.AddScoped<IWebAuthService, WebAuthService>();
builder.Services.AddSingleton<WebActiveClinicIdStore>();
builder.Services.AddSingleton<IClinicIdProvider, WebClinicIdProvider>();
builder.Services.AddScoped<IClinicContextService, ClinicContextService>();
builder.Services.AddScoped<IAccessTokenProvider, BrowserAccessTokenProvider>();
builder.Services.AddScoped<IHospitalOnboardingStorage, HospitalOnboardingStorage>();
builder.Services.AddScoped<IUiCultureService, UiCultureService>();
builder.Services.AddRaphCareClient(client => client.BaseAddress = new Uri(apiBase), useBearerToken: true);

await builder.Build().RunAsync();
