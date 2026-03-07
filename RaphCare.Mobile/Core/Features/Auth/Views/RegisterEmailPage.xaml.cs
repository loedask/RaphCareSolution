using Microsoft.Extensions.DependencyInjection;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;

namespace RaphCare.Mobile.Features.Auth.Views;

public partial class RegisterEmailPage : ContentPage
{
    public RegisterEmailPage() : this(MauiProgram.ServiceProvider!.GetRequiredService<RegisterEmailViewModel>()) { }

    public RegisterEmailPage(RegisterEmailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
