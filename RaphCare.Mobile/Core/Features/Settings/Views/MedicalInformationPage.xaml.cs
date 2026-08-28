using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class MedicalInformationPage : ContentPage
{
    public MedicalInformationPage() : this(MobileServiceHub.GetRequiredService<MedicalInformationViewModel>()) { }

    public MedicalInformationPage(MedicalInformationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MedicalInformationViewModel vm)
            await vm.LoadAsync();
    }
}
