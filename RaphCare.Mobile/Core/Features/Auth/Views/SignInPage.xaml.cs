using Microsoft.Extensions.DependencyInjection;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class SignInPage : ContentPage
{
    public SignInPage() : this(MauiProgram.ServiceProvider!.GetRequiredService<SignInViewModel>()) { }

    public SignInPage(SignInViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
