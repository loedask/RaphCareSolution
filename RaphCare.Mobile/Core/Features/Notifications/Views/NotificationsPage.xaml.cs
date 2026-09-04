using RaphCare.Mobile.Core.Features.Notifications.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Notifications.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage() : this(MobileServiceHub.GetRequiredService<NotificationsViewModel>()) { }

    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NotificationsViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
