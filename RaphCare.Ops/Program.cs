using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RaphCare.Client;
using RaphCare.Client.Contracts;
using RaphCare.Ops;
using RaphCare.Ops.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5281";
if (!apiBase.EndsWith('/'))
    apiBase += "/";

builder.Services.AddScoped<IOpsAuthService, OpsAuthService>();
builder.Services.AddScoped<IAccessTokenProvider, OpsAccessTokenProvider>();
builder.Services.AddRaphCareClient(client => client.BaseAddress = new Uri(apiBase), useBearerToken: true);

await builder.Build().RunAsync();
