using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;

public sealed class TelehealthJoinViewModel : BaseViewModel
{
    private readonly IPatientTelehealthService _telehealth;
    private readonly ITelehealthRtcSession _rtc;
    private string _fullToken = string.Empty;
    private Guid _sessionId;
    private string _channel = string.Empty;
    private string _uid = string.Empty;
    private string _appId = string.Empty;
    private string _tokenPreview = string.Empty;
    private string _rtcStatus = string.Empty;
    private string? _errorMessage;
    private string _joinChannel = string.Empty;
    private string _joinAppId = string.Empty;
    private int _joinUid;
    private bool _rtcConfigured;
    private bool _inCall;

    public TelehealthJoinViewModel(IPatientTelehealthService telehealth, ITelehealthRtcSession rtc)
    {
        _telehealth = telehealth ?? throw new ArgumentNullException(nameof(telehealth));
        _rtc = rtc ?? throw new ArgumentNullException(nameof(rtc));
        Title = T("CareTelehealthJoinTitle");
        ChannelLabel = T("CareTelehealthChannel");
        UidLabel = T("CareTelehealthUid");
        AppIdLabel = T("CareTelehealthAppId");
        TokenLabel = T("CareTelehealthToken");
        CopyTokenLabel = T("CareTelehealthCopyToken");
        SmsLabel = T("CareTelehealthSendSms");
        BackLabel = T("CareTelehealthBack");
        StartVideoLabel = T("CareTelehealthStartVideo");
        EndVideoLabel = T("CareTelehealthEndVideo");
        LocalVideoLabel = T("CareTelehealthLocalVideo");
        RemoteVideoLabel = T("CareTelehealthRemoteVideo");

        RefreshCommand = new Command(async () => await LoadAsync());
        CopyTokenCommand = new Command(async () => await CopyTokenAsync());
        SmsCommand = new Command(async () => await SendSmsAsync());
        BackCommand = new Command(async () => await GoBackAsync());
        StartVideoCommand = new Command(async () => await StartVideoAsync(), () => CanStartVideo && !InCall);
        EndVideoCommand = new Command(async () => await EndVideoAsync(), () => InCall);
    }

    public string ChannelLabel { get; }
    public string UidLabel { get; }
    public string AppIdLabel { get; }
    public string TokenLabel { get; }
    public string CopyTokenLabel { get; }
    public string SmsLabel { get; }
    public string BackLabel { get; }
    public string StartVideoLabel { get; }
    public string EndVideoLabel { get; }
    public string LocalVideoLabel { get; }
    public string RemoteVideoLabel { get; }

    public string VideoHintText =>
        DeviceInfo.Current.Platform == DevicePlatform.Android
            ? T("CareTelehealthVideoHintAndroid")
            : T("CareTelehealthVideoHintOther");

    public bool ShowVideoSection => DeviceInfo.Current.Platform == DevicePlatform.Android;

    public string ChannelText
    {
        get => _channel;
        set => SetProperty(ref _channel, value);
    }

    public string UidText
    {
        get => _uid;
        set => SetProperty(ref _uid, value);
    }

    public string AppIdText
    {
        get => _appId;
        set => SetProperty(ref _appId, value);
    }

    public string TokenPreview
    {
        get => _tokenPreview;
        set => SetProperty(ref _tokenPreview, value);
    }

    public string RtcStatusText
    {
        get => _rtcStatus;
        set => SetProperty(ref _rtcStatus, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool InCall
    {
        get => _inCall;
        set
        {
            if (_inCall == value)
                return;
            _inCall = value;
            OnPropertyChanged(nameof(InCall));
            (StartVideoCommand as Command)?.ChangeCanExecute();
            (EndVideoCommand as Command)?.ChangeCanExecute();
            OnPropertyChanged(nameof(ShowStartVideo));
            OnPropertyChanged(nameof(ShowEndVideo));
        }
    }

    public bool CanStartVideo => _rtcConfigured
        && !string.IsNullOrWhiteSpace(_joinAppId)
        && !string.IsNullOrWhiteSpace(_joinChannel)
        && !string.IsNullOrWhiteSpace(_fullToken);

    public bool ShowStartVideo => CanStartVideo && !InCall;
    public bool ShowEndVideo => InCall;

    public ICommand RefreshCommand { get; }
    public ICommand CopyTokenCommand { get; }
    public ICommand SmsCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand StartVideoCommand { get; }
    public ICommand EndVideoCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (TryGetQueryGuid(query, "sessionId", out var id))
            _sessionId = id;
    }

    /// <summary>Called from the join page when native preview surfaces are ready (Android: TextureView).</summary>
    public void BindRtcSurfaces(object? localPlatformView, object? remotePlatformView) =>
        _rtc.BindVideoSurfaces(localPlatformView, remotePlatformView);

    public async Task StopRtcAsync() => await _rtc.StopAsync(CancellationToken.None).ConfigureAwait(false);

    public async Task LoadAsync()
    {
        if (_sessionId == Guid.Empty)
        {
            ErrorMessage = T("CareTelehealthJoinFailed");
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _telehealth.GetJoinInfoAsync(_sessionId, null, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("CareTelehealthJoinFailed");
                return;
            }

            var j = response.Data;
            ChannelText = j.ChannelName;
            UidText = j.Uid.ToString(CultureInfo.InvariantCulture);
            AppIdText = string.IsNullOrEmpty(j.AppId) ? "—" : j.AppId;
            TokenPreview = string.IsNullOrEmpty(j.RtcToken)
                ? T("CareTelehealthTokenUnset")
                : j.RtcToken.Length <= 24
                    ? j.RtcToken
                    : $"{j.RtcToken[..24]}…";
            RtcStatusText = j.RtcConfigured
                ? T("CareTelehealthRtcReady")
                : T("CareTelehealthRtcNotConfigured");
            _fullToken = j.RtcToken ?? string.Empty;
            _joinChannel = j.ChannelName;
            _joinAppId = j.AppId ?? string.Empty;
            _joinUid = unchecked((int)j.Uid);
            _rtcConfigured = j.RtcConfigured;
            OnPropertyChanged(nameof(CanStartVideo));
            OnPropertyChanged(nameof(ShowStartVideo));
            (StartVideoCommand as Command)?.ChangeCanExecute();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartVideoAsync()
    {
        if (!CanStartVideo)
            return;

        IsBusy = true;
        try
        {
            var result = await _rtc.StartAsync(
                new TelehealthRtcJoinParameters(_joinAppId, _joinChannel, _fullToken, _joinUid),
                CancellationToken.None).ConfigureAwait(false);

            if (!result.Success)
            {
                await Shell.Current.DisplayAlertAsync(Title, result.ErrorMessage ?? T("CareTelehealthVideoStartFailed"), "OK");
                return;
            }

            InCall = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EndVideoAsync()
    {
        IsBusy = true;
        try
        {
            await _rtc.StopAsync(CancellationToken.None).ConfigureAwait(false);
            InCall = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoBackAsync()
    {
        await _rtc.StopAsync(CancellationToken.None).ConfigureAwait(false);
        InCall = false;
        await SafeShellNavigator.GoToAsync("..");
    }

    private async Task CopyTokenAsync()
    {
        if (string.IsNullOrEmpty(_fullToken))
        {
            await Shell.Current.DisplayAlertAsync(Title, T("CareTelehealthTokenUnset"), "OK");
            return;
        }

        await Clipboard.Default.SetTextAsync(_fullToken);
        await Shell.Current.DisplayAlertAsync(Title, T("CareTelehealthCopied"), "OK");
    }

    private async Task SendSmsAsync()
    {
        if (_sessionId == Guid.Empty) return;
        IsBusy = true;
        try
        {
            var response = await _telehealth.SendSessionSmsAsync(_sessionId, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                await Shell.Current.DisplayAlertAsync(Title, response.ErrorMessage ?? T("CareTelehealthSmsFailed"), "OK");
                return;
            }

            await Shell.Current.DisplayAlertAsync(Title, T("CareTelehealthSmsSent"), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
