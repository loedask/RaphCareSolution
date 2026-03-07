using Microsoft.Extensions.DependencyInjection;
using RaphCare.Mobile.Features.Auth.ViewModels;

namespace RaphCare.Mobile.Features.Auth.Views;

public partial class RegisterOptionsPage : ContentPage
{
    public RegisterOptionsPage() : this(MauiProgram.ServiceProvider!.GetRequiredService<RegisterOptionsViewModel>()) { }

    public RegisterOptionsPage(RegisterOptionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
