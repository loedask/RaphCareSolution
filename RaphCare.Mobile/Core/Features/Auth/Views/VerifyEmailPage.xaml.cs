using Microsoft.Extensions.DependencyInjection;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class VerifyEmailPage : ContentPage
{
    public VerifyEmailPage() : this(MauiProgram.ServiceProvider!.GetRequiredService<VerifyEmailViewModel>()) { }

    public VerifyEmailPage(VerifyEmailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
