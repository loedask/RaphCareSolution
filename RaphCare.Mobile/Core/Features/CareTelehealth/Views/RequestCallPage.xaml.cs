using RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.Views;

public partial class RequestCallPage : ContentPage
{
    public RequestCallPage() : this(MobileServiceHub.GetRequiredService<RequestCallViewModel>()) { }

    public RequestCallPage(RequestCallViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
