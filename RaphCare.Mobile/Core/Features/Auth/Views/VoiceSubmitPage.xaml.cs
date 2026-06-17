using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class VoiceSubmitPage : ContentPage
{
    public VoiceSubmitPage() : this(MobileServiceHub.GetRequiredService<VoiceSubmitViewModel>()) { }

    public VoiceSubmitPage(VoiceSubmitViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is VoiceSubmitViewModel vm)
            await vm.CancelAsync().ConfigureAwait(false);
    }
}
