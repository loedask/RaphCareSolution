using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Records.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Records.Views;

public partial class HealthRecordDetailPage : ContentPage, IQueryAttributable
{
    public HealthRecordDetailPage() : this(MobileServiceHub.GetRequiredService<HealthRecordDetailViewModel>()) { }

    public HealthRecordDetailPage(HealthRecordDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is HealthRecordDetailViewModel vm)
            vm.ApplyQueryAttributes(query);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HealthRecordDetailViewModel vm)
            await vm.LoadAsync();
    }
}
