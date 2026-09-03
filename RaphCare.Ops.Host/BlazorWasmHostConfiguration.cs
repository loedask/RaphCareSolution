namespace RaphCare.Ops.Host;

/// <summary>
/// Forwards the ASP.NET Core environment to Blazor WebAssembly and, when configured,
/// serves <c>ApiBaseUrl</c> from App Service settings instead of the baked-in wwwroot file.
/// </summary>
internal static class BlazorWasmHostConfiguration
{
    internal static void UseBlazorWasmHostConfiguration(this WebApplication app)
    {
        var environmentName = app.Environment.EnvironmentName;
        var apiBaseUrl = app.Configuration["ApiBaseUrl"];

        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(static (state) =>
            {
                var (httpContext, environment) = ((HttpContext, string))state!;
                if (!httpContext.Response.Headers.ContainsKey("Blazor-Environment"))
                    httpContext.Response.Headers["Blazor-Environment"] = environment;
                return Task.CompletedTask;
            }, (context, environmentName));

            if (HttpMethods.IsGet(context.Request.Method)
                && IsAppSettingsPath(context.Request.Path)
                && !string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                var url = apiBaseUrl.Trim().TrimEnd('/');
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.Headers.CacheControl = "no-store, no-cache";
                await context.Response.WriteAsJsonAsync(
                    new Dictionary<string, string> { ["ApiBaseUrl"] = url });
                return;
            }

            await next();
        });
    }

    internal static bool IsAppSettingsPath(PathString path)
    {
        var value = path.Value;
        if (string.IsNullOrEmpty(value))
            return false;

        if (value.Equals("/appsettings.json", StringComparison.OrdinalIgnoreCase))
            return true;

        return value.StartsWith("/appsettings.", StringComparison.OrdinalIgnoreCase)
            && value.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            && value.IndexOf('/', 1) < 0;
    }
}
