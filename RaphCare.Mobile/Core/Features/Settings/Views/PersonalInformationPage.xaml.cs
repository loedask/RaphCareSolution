using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class PersonalInformationPage : ContentPage
{
    public PersonalInformationPage() : this(MobileServiceHub.GetRequiredService<PersonalInformationViewModel>()) { }

    public PersonalInformationPage(PersonalInformationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PersonalInformationViewModel vm)
            await vm.LoadAsync();
    }
}
