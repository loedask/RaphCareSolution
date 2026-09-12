using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

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
    private bool _showConsent;
    private string _consentTitle = string.Empty;
    private string _consentBody = string.Empty;
    private bool _consentAlreadySigned;
    private string? _consentSignedLabel;
    private bool _agreeing;

    public AppointmentDetailViewModel(IAppointmentService appointments)
    {
        _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
        Title = T("AppointmentsDetailTitle");
        StatusLabel = T("AppointmentsStatus");
        TypeLabel = T("AppointmentsType");
        ReasonLabel = T("AppointmentsReason");
        WhenLabel = T("AppointmentsWhen");
        ConsentSectionTitle = T("AppointmentsConsentTitle");
        AgreeButtonText = T("AppointmentsConsentAgree");
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
        AgreeConsentCommand = new Command(async () => await AgreeConsentAsync(), () => !_agreeing && _showConsent && !_consentAlreadySigned);
    }

    public string WhenLabel { get; }
    public string StatusLabel { get; }
    public string TypeLabel { get; }
    public string ReasonLabel { get; }
    public string ConsentSectionTitle { get; }
    public string AgreeButtonText { get; }

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

    public bool ShowConsent
    {
        get => _showConsent;
        set
        {
            if (SetProperty(ref _showConsent, value))
                ((Command)AgreeConsentCommand).ChangeCanExecute();
        }
    }

    public string ConsentTitle
    {
        get => _consentTitle;
        set => SetProperty(ref _consentTitle, value);
    }

    public string ConsentBody
    {
        get => _consentBody;
        set => SetProperty(ref _consentBody, value);
    }

    public bool ConsentAlreadySigned
    {
        get => _consentAlreadySigned;
        set
        {
            if (SetProperty(ref _consentAlreadySigned, value))
                ((Command)AgreeConsentCommand).ChangeCanExecute();
        }
    }

    public string? ConsentSignedLabel
    {
        get => _consentSignedLabel;
        set => SetProperty(ref _consentSignedLabel, value);
    }

    public ICommand BackCommand { get; }
    public ICommand AgreeConsentCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!TryGetQueryGuid(query, "appointmentId", out var id))
            return;
        _appointmentId = id;
    }

    public async Task LoadAsync()
    {
        if (_appointmentId == Guid.Empty)
        {
            ErrorMessage = T("AppointmentsDetailFailed");
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _appointments.GetMyAppointmentAsync(_appointmentId, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("AppointmentsDetailFailed");
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

            await LoadConsentAsync().ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadConsentAsync()
    {
        var response = await _appointments.GetConsentAsync(_appointmentId, CancellationToken.None).ConfigureAwait(false);
        if (!response.IsSuccess || response.Data is null)
        {
            ShowConsent = false;
            return;
        }

        var consent = response.Data;
        if (!consent.NeedsConsent && !consent.AlreadySigned)
        {
            ShowConsent = false;
            return;
        }

        ShowConsent = true;
        ConsentTitle = consent.Title;
        ConsentBody = consent.Body;
        ConsentAlreadySigned = consent.AlreadySigned;
        ConsentSignedLabel = consent.AlreadySigned ? T("AppointmentsConsentSigned") : null;
    }

    private async Task AgreeConsentAsync()
    {
        if (_appointmentId == Guid.Empty || ConsentAlreadySigned)
            return;

        _agreeing = true;
        ((Command)AgreeConsentCommand).ChangeCanExecute();
        ErrorMessage = null;
        try
        {
            var response = await _appointments.AgreeConsentAsync(_appointmentId, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("AppointmentsConsentAgreeFailed");
                return;
            }

            ConsentAlreadySigned = true;
            ConsentTitle = response.Data.Title;
            ConsentBody = response.Data.Body;
            ConsentSignedLabel = T("AppointmentsConsentSigned");
        }
        finally
        {
            _agreeing = false;
            ((Command)AgreeConsentCommand).ChangeCanExecute();
        }
    }
}
