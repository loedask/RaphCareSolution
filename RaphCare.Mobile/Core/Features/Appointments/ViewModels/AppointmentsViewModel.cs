using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Appointments;
using RaphCare.Mobile.Core.Features.Appointments.Models;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Appointments.ViewModels;

public sealed class AppointmentsViewModel : BaseViewModel
{
    private readonly IAppointmentService _appointments;
    private string? _errorMessage;

    public AppointmentsViewModel(IAppointmentService appointments)
    {
        _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
        Title = T("AppointmentsListTitle");
        BookButtonText = T("AppointmentsBook");
        RefreshButtonText = T("AppointmentsRefresh");
        EmptyStateText = T("AppointmentsEmpty");

        RefreshCommand = new Command(async () => await LoadAsync());
        BookCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.BookAppointment));
        OpenDetailCommand = new Command<Guid>(async id => await SafeShellNavigator.GoToAsync($"{AppNavigator.AppointmentDetail}?appointmentId={id}"));

        Items.CollectionChanged += (_, _) => NotifyEmptyChanged();
    }

    public ObservableCollection<AppointmentListDisplayItem> Items { get; } = new();

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowEmpty => !IsBusy && Items.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public string BookButtonText { get; }
    public string RefreshButtonText { get; }
    public string EmptyStateText { get; }

    public ICommand RefreshCommand { get; }
    public ICommand BookCommand { get; }
    public ICommand OpenDetailCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        NotifyEmptyChanged();
        try
        {
            var response = await _appointments.GetMyAppointmentsAsync(1, 50, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("AppointmentsLoadFailed");
                Items.Clear();
                NotifyEmptyChanged();
                return;
            }

            Items.Clear();
            var culture = CultureInfo.CurrentCulture;
            foreach (var a in response.Data.Items)
                Items.Add(MapItem(a, culture));
        }
        finally
        {
            IsBusy = false;
            NotifyEmptyChanged();
        }
    }

    private void NotifyEmptyChanged()
    {
        OnPropertyChanged(nameof(ShowEmpty));
    }

    private static AppointmentListDisplayItem MapItem(AppointmentViewModel a, CultureInfo culture)
    {
        var start = a.ScheduledStart.ToLocalTime();
        var end = a.ScheduledEnd.ToLocalTime();
        var when = $"{start.ToString("g", culture)} – {end.ToString("t", culture)}";
        var secondary = string.IsNullOrWhiteSpace(a.Reason)
            ? a.Status
            : $"{a.Status} · {a.Reason}";
        return new AppointmentListDisplayItem
        {
            Id = a.Id,
            PrimaryLine = $"{when} · {a.Type}",
            SecondaryLine = secondary,
            Status = a.Status
        };
    }
}
