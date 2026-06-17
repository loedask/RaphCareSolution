using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class RegisterVoiceIntroPage : ContentPage
{
    public RegisterVoiceIntroPage() : this(MobileServiceHub.GetRequiredService<RegisterVoiceIntroViewModel>()) { }

    public RegisterVoiceIntroPage(RegisterVoiceIntroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
