using RaphCare.Mobile.Core.Features.Insurance.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Insurance.Views;

public partial class AddInsuranceProfilePage : ContentPage
{
    public AddInsuranceProfilePage() : this(MobileServiceHub.GetRequiredService<AddInsuranceProfileViewModel>()) { }

    public AddInsuranceProfilePage(AddInsuranceProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AddInsuranceProfileViewModel vm)
            await vm.LoadPlansAsync();
    }
}
