using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class RegisterOptionsPage : ContentPage
{
    public RegisterOptionsPage() : this(MobileServiceHub.GetRequiredService<RegisterOptionsViewModel>()) { }

    public RegisterOptionsPage(RegisterOptionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
