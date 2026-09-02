using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.Views;

public partial class TelehealthJoinPage : ContentPage, IQueryAttributable
{
    public TelehealthJoinPage() : this(MobileServiceHub.GetRequiredService<TelehealthJoinViewModel>()) { }

    public TelehealthJoinPage(TelehealthJoinViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TelehealthJoinViewModel.InCall))
                TryBindRtcSurfaces();
        };
        LocalPreview.HandlerChanged += (_, _) => TryBindRtcSurfaces();
        RemotePreview.HandlerChanged += (_, _) => TryBindRtcSurfaces();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is TelehealthJoinViewModel vm)
            vm.ApplyQueryAttributes(query);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SafePageLoad.RunAsync(async () =>
        {
            if (BindingContext is TelehealthJoinViewModel vm)
            {
                await vm.LoadAsync();
                TryBindRtcSurfaces();
                await Task.Delay(250).ConfigureAwait(true);
                TryBindRtcSurfaces();
            }
        });
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is TelehealthJoinViewModel vm)
            await vm.StopRtcAsync().ConfigureAwait(false);
    }

    private void TryBindRtcSurfaces()
    {
        if (BindingContext is not TelehealthJoinViewModel vm)
            return;

        vm.BindRtcSurfaces(LocalPreview.Handler?.PlatformView, RemotePreview.Handler?.PlatformView);
    }
}
