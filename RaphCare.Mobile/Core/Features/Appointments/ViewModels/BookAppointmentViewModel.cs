using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Appointments;
using RaphCare.Mobile.Core.Common.Configuration;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.Api;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Appointments.ViewModels;

public sealed class BookAppointmentViewModel : BaseViewModel
{
    private readonly IAppointmentService _appointments;
    private readonly ISelectedClinicStore _selectedClinic;
    private readonly IClinicIdProvider _clinicIdProvider;
    private readonly AppointmentsMobileOptions _defaults;

    private string _clinicSummary = string.Empty;
    private ProviderPickerItem? _selectedProvider;
    private DateTime _appointmentDate = DateTime.Today.AddDays(1);
    private TimeSpan _startTime = new(9, 0, 0);
    private TimeSpan _endTime = new(9, 30, 0);
    private string _selectedType = "InPerson";
    private string _reason = string.Empty;
    private string? _errorMessage;
    private bool _hasClinic;

    public BookAppointmentViewModel(
        IAppointmentService appointments,
        ISelectedClinicStore selectedClinic,
        IClinicIdProvider clinicIdProvider,
        IOptions<AppointmentsMobileOptions> options)
    {
        _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
        _selectedClinic = selectedClinic ?? throw new ArgumentNullException(nameof(selectedClinic));
        _clinicIdProvider = clinicIdProvider ?? throw new ArgumentNullException(nameof(clinicIdProvider));
        _defaults = options?.Value ?? new AppointmentsMobileOptions();
        Title = T("AppointmentsBookTitle");

        PageSubtitle = T("AppointmentsBookSubtitle");
        ClinicLabel = T("AppointmentsClinicLabel");
        ChangeClinicLabel = T("AppointmentsChangeClinic");
        ProviderLabel = T("AppointmentsProviderLabel");
        DateLabel = T("AppointmentsDate");
        StartLabel = T("AppointmentsStartTime");
        EndLabel = T("AppointmentsEndTime");
        TypeLabel = T("AppointmentsType");
        ReasonLabel = T("AppointmentsReason");
        SubmitLabel = T("AppointmentsSubmit");
        CancelLabel = T("AppointmentsCancel");

        Providers = new ObservableCollection<ProviderPickerItem>();

        SubmitCommand = new Command(async () => await SubmitAsync(), () => !IsBusy && HasClinic);
        CancelCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
        ChangeClinicCommand = new Command(async () =>
            await SafeShellNavigator.GoToAsync(AppNavigator.SelectClinic));
        RefreshCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
    }

    public string SubmitLabel { get; }
    public string CancelLabel { get; }
    public string PageSubtitle { get; }
    public string ClinicLabel { get; }
    public string ChangeClinicLabel { get; }
    public string ProviderLabel { get; }
    public string DateLabel { get; }
    public string StartLabel { get; }
    public string EndLabel { get; }
    public string TypeLabel { get; }
    public string ReasonLabel { get; }

    public IReadOnlyList<string> VisitTypes { get; } = ["InPerson", "Telemedicine"];

    public ObservableCollection<ProviderPickerItem> Providers { get; }

    public string ClinicSummary
    {
        get => _clinicSummary;
        private set => SetProperty(ref _clinicSummary, value);
    }

    public bool HasClinic
    {
        get => _hasClinic;
        private set
        {
            if (_hasClinic == value)
                return;
            _hasClinic = value;
            OnPropertyChanged();
            (SubmitCommand as Command)?.ChangeCanExecute();
        }
    }

    public ProviderPickerItem? SelectedProvider
    {
        get => _selectedProvider;
        set => SetProperty(ref _selectedProvider, value);
    }

    public DateTime AppointmentDate
    {
        get => _appointmentDate;
        set => SetProperty(ref _appointmentDate, value);
    }

    public TimeSpan StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    public TimeSpan EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    public string SelectedType
    {
        get => _selectedType;
        set => SetProperty(ref _selectedType, value ?? "InPerson");
    }

    public string Reason
    {
        get => _reason;
        set => SetProperty(ref _reason, value ?? string.Empty);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ChangeClinicCommand { get; }
    public ICommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        ErrorMessage = null;
        RefreshClinicSummary();

        if (!HasClinic)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Providers.Clear();
                SelectedProvider = null;
            });
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _appointments
                .GetBookableProvidersAsync(CancellationToken.None)
                .ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Providers.Clear();
                if (!response.IsSuccess || response.Data is null)
                {
                    ErrorMessage = response.ErrorMessage ?? T("AppointmentsProvidersLoadFailed");
                    SelectedProvider = null;
                    return;
                }

                foreach (var provider in response.Data)
                {
                    Providers.Add(new ProviderPickerItem
                    {
                        Id = provider.Id,
                        DisplayName = string.IsNullOrWhiteSpace(provider.DisplayName)
                            ? T("AppointmentsProviderFallback")
                            : provider.DisplayName
                    });
                }

                if (Providers.Count == 0)
                {
                    ErrorMessage = T("AppointmentsNoProviders");
                    SelectedProvider = null;
                    return;
                }

                SelectedProvider = PickDefaultProvider();
            });
        }
        catch (Exception)
        {
            ErrorMessage = T("AppointmentsProvidersLoadFailed");
        }
        finally
        {
            IsBusy = false;
            (SubmitCommand as Command)?.ChangeCanExecute();
            (RefreshCommand as Command)?.ChangeCanExecute();
        }
    }

    private void RefreshClinicSummary()
    {
        var clinicId = _clinicIdProvider.GetClinicId();
        HasClinic = clinicId is { } id && id != Guid.Empty;

        if (!HasClinic)
        {
            ClinicSummary = T("AppointmentsNoClinicSelected");
            return;
        }

        var name = _selectedClinic.ClinicName;
        var code = _selectedClinic.ReferenceCode;
        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(code))
            ClinicSummary = Format(T("AppointmentsClinicSummaryWithCodeFormat"), name, code);
        else if (!string.IsNullOrWhiteSpace(name))
            ClinicSummary = name;
        else
            ClinicSummary = T("AppointmentsClinicConfigured");
    }

    private ProviderPickerItem? PickDefaultProvider()
    {
        if (Providers.Count == 0)
            return null;

        if (Guid.TryParse(_defaults.DefaultProviderId, out var defaultId))
        {
            var match = Providers.FirstOrDefault(p => p.Id == defaultId);
            if (match is not null)
                return match;
        }

        return Providers[0];
    }

    private async Task SubmitAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;

        var clinicId = _clinicIdProvider.GetClinicId();
        if (clinicId is null || clinicId == Guid.Empty)
        {
            ErrorMessage = T("AppointmentsNoClinicSelected");
            return;
        }

        if (SelectedProvider is null)
        {
            ErrorMessage = T("AppointmentsSelectProvider");
            return;
        }

        var day = AppointmentDate.Date;
        var start = day.Add(StartTime);
        var end = day.Add(EndTime);
        if (end <= start)
        {
            ErrorMessage = T("AppointmentsEndBeforeStart");
            return;
        }

        IsBusy = true;
        try
        {
            var request = new BookAppointmentRequest
            {
                ClinicId = clinicId.Value,
                ProviderId = SelectedProvider.Id,
                ScheduledStart = DateTime.SpecifyKind(start, DateTimeKind.Local).ToUniversalTime(),
                ScheduledEnd = DateTime.SpecifyKind(end, DateTimeKind.Local).ToUniversalTime(),
                Type = SelectedType,
                Reason = Reason.Trim()
            };

            var response = await _appointments.BookAsync(request, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                ErrorMessage = response.ErrorMessage ?? T("AppointmentsBookingFailed");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlertAsync(Title, T("AppointmentsBookingOk"), T("CommonOk"));
                await SafeShellNavigator.GoToAsync("..");
            });
        }
        finally
        {
            IsBusy = false;
            (SubmitCommand as Command)?.ChangeCanExecute();
            (RefreshCommand as Command)?.ChangeCanExecute();
        }
    }
}

public sealed class ProviderPickerItem
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}
