using Microsoft.Extensions.DependencyInjection;
using RaphCare.Client.Contracts;
using RaphCare.Web.Services;
using Xunit;

namespace RaphCare.Web.Tests.Services;

/// <summary>
/// Guards against scoped clinic providers: HttpClient handlers must see the same clinic id the UI set.
/// </summary>
public sealed class WebClinicIdProviderTests
{
    [Fact]
    public void SingletonStoreSharesClinicIdBetweenUiWriteAndHandlerRead()
    {
        var services = new ServiceCollection();
        services.AddSingleton<WebActiveClinicIdStore>();
        services.AddSingleton<IClinicIdProvider, WebClinicIdProvider>();
        using var provider = services.BuildServiceProvider();

        var clinicId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        // UI path: ClinicContextService writes through the store.
        var store = provider.GetRequiredService<WebActiveClinicIdStore>();
        store.ClinicId = clinicId;

        // HttpClient handler path: resolves IClinicIdProvider separately (same singleton store).
        var fromHandler = provider.GetRequiredService<IClinicIdProvider>().GetClinicId();

        Assert.Equal(clinicId, fromHandler);
    }

    [Fact]
    public void EmptyGuidIsTreatedAsNoClinic()
    {
        var store = new WebActiveClinicIdStore { ClinicId = Guid.Empty };
        var clinicProvider = new WebClinicIdProvider(store);

        Assert.Null(clinicProvider.GetClinicId());
    }
}
