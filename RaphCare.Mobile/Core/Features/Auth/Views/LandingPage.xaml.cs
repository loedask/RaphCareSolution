using Microsoft.Extensions.DependencyInjection;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;

namespace RaphCare.Mobile.Features.Auth.Views;

public partial class LandingPage : ContentPage
{
    public LandingPage() : this(MauiProgram.ServiceProvider!.GetRequiredService<LandingViewModel>()) { }

    public LandingPage(LandingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
