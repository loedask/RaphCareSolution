using RaphCare.Mobile.Core.Features.Insurance.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Insurance.Views;

public partial class InsurancePage : ContentPage
{
    public InsurancePage() : this(MobileServiceHub.GetRequiredService<InsuranceViewModel>()) { }

    public InsurancePage(InsuranceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is InsuranceViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
