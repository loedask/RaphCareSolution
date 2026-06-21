using RaphCare.Client.Models.Support;
using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class HelpFaqPage : ContentPage
{
    public HelpFaqPage() : this(MobileServiceHub.GetRequiredService<HelpFaqViewModel>()) { }

    public HelpFaqPage(HelpFaqViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HelpFaqViewModel vm)
            await vm.LoadAsync();
    }

    private async void OnFaqSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not HelpFaqViewModel vm)
            return;

        if (e.CurrentSelection.Count == 0 || e.CurrentSelection[0] is not PatientSupportFaqItemViewModel item)
            return;

        FaqList.SelectedItem = null;
        await HelpFaqViewModel.OpenItemAsync(item);
    }
}
