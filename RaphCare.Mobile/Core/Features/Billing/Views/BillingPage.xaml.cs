using RaphCare.Mobile.Core.Features.Billing.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Billing.Views;

public partial class BillingPage : ContentPage
{
    public BillingPage() : this(MobileServiceHub.GetRequiredService<BillingViewModel>()) { }

    public BillingPage(BillingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BillingViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
