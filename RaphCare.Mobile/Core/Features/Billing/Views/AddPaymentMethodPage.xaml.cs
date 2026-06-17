using RaphCare.Mobile.Core.Features.Billing.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Billing.Views;

public partial class AddPaymentMethodPage : ContentPage
{
    public AddPaymentMethodPage() : this(MobileServiceHub.GetRequiredService<AddPaymentMethodViewModel>()) { }

    public AddPaymentMethodPage(AddPaymentMethodViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
