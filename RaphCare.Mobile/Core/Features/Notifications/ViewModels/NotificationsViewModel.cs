using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Notifications;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Notifications.ViewModels;

/// <summary>Patient notification center (concept <c>/notifications</c>).</summary>
public sealed class NotificationsViewModel : BaseViewModel
{
    private readonly IPatientNotificationsService _notifications;
    private string? _errorMessage;

    public NotificationsViewModel(IPatientNotificationsService notifications)
    {
        _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        Title = AppResources.T("NotificationsTitle");
        Items = new ObservableCollection<PatientNotificationViewModel>();
        Items.CollectionChanged += OnItemsCollectionChanged;

        RefreshCommand = new Command(async () => await LoadAsync());
        MarkAllReadCommand = new Command(async () => await MarkAllReadAsync());
        OpenItemCommand = new Command<Guid>(async id => await OpenItemAsync(id));
    }

    public ObservableCollection<PatientNotificationViewModel> Items { get; }

    public string RefreshButtonText => AppResources.T("NotificationsRefresh");
    public string MarkAllReadButtonText => AppResources.T("NotificationsMarkAllRead");
    public string EmptyStateText => AppResources.T("NotificationsEmpty");
    public string PushHintText => AppResources.T("NotificationsPushHint");
    public string NewBadgeText => AppResources.T("NotificationsNewBadge");

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowEmpty => !IsBusy && Items.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public ICommand RefreshCommand { get; }
    public ICommand MarkAllReadCommand { get; }
    public ICommand OpenItemCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _notifications.GetMyNotificationsAsync(CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? AppResources.T("NotificationsLoadFailed");
                Items.Clear();
                return;
            }

            Items.Clear();
            foreach (var row in response.Data)
                Items.Add(row);
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(ShowEmpty));
        }
    }

    private async Task MarkAllReadAsync()
    {
        if (Items.Count == 0) return;
        ErrorMessage = null;
        var response = await _notifications.MarkAllReadAsync(CancellationToken.None).ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            ErrorMessage = response.ErrorMessage ?? AppResources.T("NotificationsLoadFailed");
            return;
        }

        await LoadAsync().ConfigureAwait(false);
    }

    private async Task OpenItemAsync(Guid id)
    {
        ErrorMessage = null;
        var response = await _notifications.MarkReadAsync(id, CancellationToken.None).ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            ErrorMessage = response.ErrorMessage ?? AppResources.T("NotificationsLoadFailed");
            return;
        }

        await LoadAsync().ConfigureAwait(false);
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowEmpty));
    }
}
