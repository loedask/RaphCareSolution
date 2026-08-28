using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class LanguageSettingsPage : ContentPage
{
    public LanguageSettingsPage() : this(MobileServiceHub.GetRequiredService<LanguageSettingsViewModel>()) { }

    public LanguageSettingsPage(LanguageSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
