using RaphCare.Mobile.Core.Features.MentalHealth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.MentalHealth.Views;

public partial class MentalHealthPage : ContentPage
{
    public MentalHealthPage() : this(MobileServiceHub.GetRequiredService<MentalHealthViewModel>()) { }

    public MentalHealthPage(MentalHealthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MentalHealthViewModel vm)
            await vm.LoadAsync();
    }
}
