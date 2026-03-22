using Microsoft.Extensions.DependencyInjection;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class RegisterOptionsPage : ContentPage
{
    public RegisterOptionsPage() : this(MauiProgram.ServiceProvider!.GetRequiredService<RegisterOptionsViewModel>()) { }

    public RegisterOptionsPage(RegisterOptionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
