using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class SupportMessagePage : ContentPage
{
    public SupportMessagePage() : this(MobileServiceHub.GetRequiredService<SupportMessageViewModel>()) { }

    public SupportMessagePage(SupportMessageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
