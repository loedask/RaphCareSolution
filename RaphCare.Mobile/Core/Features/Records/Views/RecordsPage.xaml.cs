using RaphCare.Mobile.Core.Features.Records.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Records.Views;

public partial class RecordsPage : ContentPage
{
    public RecordsPage() : this(MobileServiceHub.GetRequiredService<RecordsViewModel>()) { }

    public RecordsPage(RecordsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RecordsViewModel vm)
            await vm.LoadAsync();
    }
}
