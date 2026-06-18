using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RaphCare.Client;
using RaphCare.Client.Contracts;
using RaphCare.Web;
using RaphCare.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5281";
if (!apiBase.EndsWith('/'))
    apiBase += "/";

builder.Services.AddScoped<IWebAuthService, WebAuthService>();
builder.Services.AddScoped<IAccessTokenProvider, BrowserAccessTokenProvider>();
builder.Services.AddRaphCareClient(client => client.BaseAddress = new Uri(apiBase), useBearerToken: true);

await builder.Build().RunAsync();
