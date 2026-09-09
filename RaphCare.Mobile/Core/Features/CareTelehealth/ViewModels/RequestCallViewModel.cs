using System.Globalization;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Configuration;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;

/// <summary>Request a telehealth call (concept <c>RequestCall.tsx</c>). Creates an on-demand Agora session via API.</summary>
public sealed class RequestCallViewModel : BaseViewModel
{
    private readonly IPatientTelehealthService _telehealth;
    private readonly AppointmentsMobileOptions _appointments;
    private string _state = "idle";
    private string _mode = "video";

    public RequestCallViewModel(
        IPatientTelehealthService telehealth,
        IOptions<AppointmentsMobileOptions> appointments)
    {
        _telehealth = telehealth ?? throw new ArgumentNullException(nameof(telehealth));
        _appointments = appointments.Value;
        Title = T("RequestCallTitle");
        VideoModeText = T("RequestCallModeVideo");
        AudioModeText = T("RequestCallModeAudio");
        LowBandModeText = T("RequestCallModeLowBand");
        StartButtonText = T("RequestCallStart");
        BackButtonText = T("CommonCancel");
        ModePickerLabel = T("RequestCallModePickerLabel");

        SelectVideoCommand = new Command(() => Mode = "video");
        SelectAudioCommand = new Command(() => Mode = "audio");
        SelectLowBandCommand = new Command(() => Mode = "lowband");
        StartCommand = new Command(async () => await StartCallFlowAsync());
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
    }

    public string VideoModeText { get; }
    public string AudioModeText { get; }
    public string LowBandModeText { get; }
    public string StartButtonText { get; }
    public string BackButtonText { get; }
    public string ModePickerLabel { get; }

    public string State
    {
        get => _state;
        private set
        {
            SetProperty(ref _state, value);
            OnPropertyChanged(nameof(StateTitle));
            OnPropertyChanged(nameof(StateSubtitle));
            OnPropertyChanged(nameof(IsIdle));
            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(ShowStartButton));
        }
    }

    public string Mode
    {
        get => _mode;
        private set
        {
            SetProperty(ref _mode, value);
            OnPropertyChanged(nameof(IsVideoMode));
            OnPropertyChanged(nameof(IsAudioMode));
            OnPropertyChanged(nameof(IsLowBandMode));
        }
    }

    public bool IsVideoMode => Mode == "video";
    public bool IsAudioMode => Mode == "audio";
    public bool IsLowBandMode => Mode == "lowband";
    public bool IsIdle => State == "idle";
    public bool IsConnecting => State is "checking" or "connecting" or "queue" or "connected";
    public bool ShowStartButton => IsIdle;

    public string StateTitle => State switch
    {
        "checking" => T("RequestCallChecking"),
        "connecting" => T("RequestCallConnecting"),
        "queue" => T("RequestCallQueue"),
        "connected" => T("RequestCallConnected"),
        _ => T("RequestCallIdleTitle"),
    };

    public string StateSubtitle => State switch
    {
        "checking" => T("RequestCallCheckingSub"),
        "connecting" => T("RequestCallConnectingSub"),
        "queue" => T("RequestCallQueueSub"),
        "connected" => T("RequestCallConnectedSub"),
        _ => T("RequestCallIdleSub"),
    };

    public ICommand SelectVideoCommand { get; }
    public ICommand SelectAudioCommand { get; }
    public ICommand SelectLowBandCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand BackCommand { get; }

    private async Task StartCallFlowAsync()
    {
        State = "checking";
        await Task.Delay(800);

        var clinicId = ParseGuid(_appointments.DefaultClinicId);
        var providerId = ParseGuid(_appointments.DefaultProviderId);

        State = "connecting";
        var response = await _telehealth
            .RequestOnDemandSessionAsync(clinicId, providerId, Mode, CancellationToken.None)
            .ConfigureAwait(false);

        if (!response.IsSuccess || response.Data == Guid.Empty)
        {
            State = "idle";
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(
                    Title,
                    response.ErrorMessage ?? T("RequestCallFailed"),
                    T("CommonOk")));
            return;
        }

        State = "queue";
        await Task.Delay(1200);
        State = "connected";
        await Task.Delay(600);

        var sessionId = response.Data.ToString("D", CultureInfo.InvariantCulture);
        await SafeShellNavigator.GoToAsync($"{AppNavigator.TelehealthJoin}?sessionId={sessionId}");
    }

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
}
