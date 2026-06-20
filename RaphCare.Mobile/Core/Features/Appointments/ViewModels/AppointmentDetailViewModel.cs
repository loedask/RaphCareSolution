using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Appointments.ViewModels;

public sealed class AppointmentDetailViewModel : BaseViewModel
{
    private readonly IAppointmentService _appointments;
    private Guid _appointmentId;
    private string _when = string.Empty;
    private string _status = string.Empty;
    private string _type = string.Empty;
    private string _reason = string.Empty;
    private string? _errorMessage;

    public AppointmentDetailViewModel(IAppointmentService appointments)
    {
        _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
        Title = AppResources.T("AppointmentsDetailTitle");
        StatusLabel = AppResources.T("AppointmentsStatus");
        TypeLabel = AppResources.T("AppointmentsType");
        ReasonLabel = AppResources.T("AppointmentsReason");
        WhenLabel = AppResources.T("AppointmentsWhen");
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
    }

    public string WhenLabel { get; }
    public string StatusLabel { get; }
    public string TypeLabel { get; }
    public string ReasonLabel { get; }

    public string WhenText
    {
        get => _when;
        set => SetProperty(ref _when, value);
    }

    public string StatusText
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string TypeText
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public string ReasonText
    {
        get => _reason;
        set => SetProperty(ref _reason, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("appointmentId", out var v) && v != null && Guid.TryParse(v.ToString(), out var id))
            _appointmentId = id;
    }

    public async Task LoadAsync()
    {
        if (_appointmentId == Guid.Empty)
        {
            ErrorMessage = AppResources.T("AppointmentsDetailFailed");
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _appointments.GetMyAppointmentAsync(_appointmentId, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? AppResources.T("AppointmentsDetailFailed");
                return;
            }

            var a = response.Data;
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var start = a.ScheduledStart.ToLocalTime();
            var end = a.ScheduledEnd.ToLocalTime();
            WhenText = $"{start.ToString("F", culture)} – {end.ToString("t", culture)}";
            StatusText = a.Status;
            TypeText = a.Type;
            ReasonText = string.IsNullOrWhiteSpace(a.Reason) ? "—" : a.Reason!;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
