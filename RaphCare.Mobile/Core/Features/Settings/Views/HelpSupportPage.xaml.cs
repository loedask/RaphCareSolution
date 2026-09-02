using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class HelpSupportPage : ContentPage
{
    public HelpSupportPage() : this(MobileServiceHub.GetRequiredService<HelpSupportViewModel>()) { }

    public HelpSupportPage(HelpSupportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HelpSupportViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
