using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class LandingPage : ContentPage
{
    public LandingPage() : this(MobileServiceHub.GetRequiredService<LandingViewModel>()) { }

    public LandingPage(LandingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
