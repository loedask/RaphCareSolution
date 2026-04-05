using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class PrivacyPage : ContentPage
{
    public PrivacyPage() : this(MobileServiceHub.GetRequiredService<PrivacySettingsViewModel>()) { }

    public PrivacyPage(PrivacySettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PrivacySettingsViewModel vm)
            vm.LoadFromStore();
    }
}
