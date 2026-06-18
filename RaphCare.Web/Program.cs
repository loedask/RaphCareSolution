using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RaphCare.Client;
using RaphCare.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5281";
if (!apiBase.EndsWith('/'))
    apiBase += "/";

builder.Services.AddRaphCareClient(client => client.BaseAddress = new Uri(apiBase));

await builder.Build().RunAsync();
