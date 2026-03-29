using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.Views;

public partial class TelehealthJoinPage : ContentPage, IQueryAttributable
{
    public TelehealthJoinPage() : this(MobileServiceHub.GetRequiredService<TelehealthJoinViewModel>()) { }

    public TelehealthJoinPage(TelehealthJoinViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is TelehealthJoinViewModel vm)
            vm.ApplyQueryAttributes(query);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TelehealthJoinViewModel vm)
            await vm.LoadAsync();
    }
}
