using System.Globalization;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Appointments;
using RaphCare.Mobile.Core.Common.Configuration;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Appointments.ViewModels;

public sealed class BookAppointmentViewModel : BaseViewModel
{
    private readonly IAppointmentService _appointments;
    private readonly AppointmentsMobileOptions _defaults;

    private string _clinicIdText = string.Empty;
    private string _providerIdText = string.Empty;
    private DateTime _appointmentDate = DateTime.Today.AddDays(1);
    private TimeSpan _startTime = new(9, 0, 0);
    private TimeSpan _endTime = new(9, 30, 0);
    private string _selectedType = "InPerson";
    private string _reason = string.Empty;
    private string? _errorMessage;

    public BookAppointmentViewModel(IAppointmentService appointments, IOptions<AppointmentsMobileOptions> options)
    {
        _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
        _defaults = options?.Value ?? new AppointmentsMobileOptions();
        Title = T("AppointmentsBookTitle");
        SubmitLabel = T("AppointmentsSubmit");
        CancelLabel = T("AppointmentsCancel");

        if (Guid.TryParse(_defaults.DefaultClinicId, out var c))
            ClinicIdText = c.ToString("D", CultureInfo.InvariantCulture);
        if (Guid.TryParse(_defaults.DefaultProviderId, out var p))
            ProviderIdText = p.ToString("D", CultureInfo.InvariantCulture);

        SubmitCommand = new Command(async () => await SubmitAsync());
        CancelCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
    }

    public string SubmitLabel { get; }
    public string CancelLabel { get; }

    public string PageSubtitle => T("AppointmentsBookSubtitle");

    public string ClinicIdLabel => T("AppointmentsClinicId");
    public string ProviderIdLabel => T("AppointmentsProviderId");
    public string DateLabel => T("AppointmentsDate");
    public string StartLabel => T("AppointmentsStartTime");
    public string EndLabel => T("AppointmentsEndTime");
    public string TypeLabel => T("AppointmentsType");
    public string ReasonLabel => T("AppointmentsReason");

    public IReadOnlyList<string> VisitTypes { get; } = new[] { "InPerson", "Telemedicine" };

    public string ClinicIdText
    {
        get => _clinicIdText;
        set => SetProperty(ref _clinicIdText, value ?? string.Empty);
    }

    public string ProviderIdText
    {
        get => _providerIdText;
        set => SetProperty(ref _providerIdText, value ?? string.Empty);
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

    private async Task SubmitAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;

        if (!Guid.TryParse(ClinicIdText.Trim(), out var clinicId) || !Guid.TryParse(ProviderIdText.Trim(), out var providerId))
        {
            ErrorMessage = T("AppointmentsInvalidGuid");
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
                ClinicId = clinicId,
                ProviderId = providerId,
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
                await Shell.Current.DisplayAlertAsync(Title, T("AppointmentsBookingOk"), "OK");
                await SafeShellNavigator.GoToAsync("..");
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
