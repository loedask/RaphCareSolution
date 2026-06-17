using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Insurance.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Insurance.Views;

public partial class InsuranceProfileDetailPage : ContentPage, IQueryAttributable
{
    public InsuranceProfileDetailPage() : this(MobileServiceHub.GetRequiredService<InsuranceProfileDetailViewModel>()) { }

    public InsuranceProfileDetailPage(InsuranceProfileDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is InsuranceProfileDetailViewModel vm)
            vm.ApplyQueryAttributes(query);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is InsuranceProfileDetailViewModel vm)
            await vm.LoadAsync();
    }
}
