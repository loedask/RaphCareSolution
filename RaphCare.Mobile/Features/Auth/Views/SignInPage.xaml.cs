using Microsoft.Extensions.DependencyInjection;
using RaphCare.Mobile.Features.Auth.ViewModels;

namespace RaphCare.Mobile.Features.Auth.Views;

public partial class SignInPage : ContentPage
{
    public SignInPage() : this(MauiProgram.ServiceProvider!.GetRequiredService<SignInViewModel>()) { }

    public SignInPage(SignInViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
