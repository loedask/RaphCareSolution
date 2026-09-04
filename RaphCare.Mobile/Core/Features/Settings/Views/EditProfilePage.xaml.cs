using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage() : this(MobileServiceHub.GetRequiredService<EditProfileViewModel>()) { }

    public EditProfilePage(EditProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EditProfileViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
