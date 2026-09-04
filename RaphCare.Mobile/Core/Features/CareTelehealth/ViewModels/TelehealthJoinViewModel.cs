using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Telehealth;
using RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;

public sealed class TelehealthJoinViewModel : BaseViewModel, IDisposable
{
    private readonly IPatientTelehealthService _telehealth;
    private readonly ITelehealthRtcSession _rtc;
    private readonly bool _showVideoSection;
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
    private bool _isMuted;
    private bool _isVideoOff;
    private string _callDurationText = "00:00";
    private string _providerDisplayName = string.Empty;
    private string _connectionBadgeText = string.Empty;
    private bool _remoteParticipantPresent;
    private bool _isChatOpen;
    private string _chatInput = string.Empty;
    private System.Threading.Timer? _chatPollTimer;
    private System.Threading.Timer? _callTimer;
    private DateTimeOffset _callStarted;

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
        StartVideoLabel = T("ConsultationJoin");
        EndVideoLabel = T("CareTelehealthEndVideo");
        LocalVideoLabel = T("CareTelehealthLocalVideo");
        RemoteVideoLabel = T("CareTelehealthRemoteVideo");
        ProviderSubtitle = T("ConsultationProviderSubtitle");
        MuteLabel = T("ConsultationMute");
        UnmuteLabel = T("ConsultationUnmute");
        VideoOnLabel = T("ConsultationVideoOn");
        VideoOffLabel = T("ConsultationVideoOff");
        ChatLabel = T("ConsultationChat");
        HangUpLabel = T("ConsultationHangUp");
        JoinPromptText = T("ConsultationJoinPrompt");
        _providerDisplayName = T("ConsultationProviderDefault");
        _connectionBadgeText = T("ConsultationConnecting");
        _showVideoSection = DeviceInfo.Current.Platform == DevicePlatform.Android;

        ChatSendLabel = T("ConsultationChatSend");
        ChatCloseLabel = T("ConsultationChatClose");
        ChatEmptyText = T("ConsultationChatEmpty");
        ChatMessages = new ObservableCollection<TelehealthChatMessageViewModel>();

        _rtc.ChannelJoined += OnRtcChannelJoined;
        _rtc.RemoteUserJoined += OnRtcRemoteUserJoined;
        _rtc.RemoteUserLeft += OnRtcRemoteUserLeft;

        RefreshCommand = new Command(async () => await LoadAsync());
        CopyTokenCommand = new Command(async () => await CopyTokenAsync());
        SmsCommand = new Command(async () => await SendSmsAsync());
        BackCommand = new Command(async () => await GoBackAsync());
        StartVideoCommand = new Command(async () => await StartVideoAsync(), () => CanStartVideo && !InCall);
        EndVideoCommand = new Command(async () => await EndVideoAsync(), () => InCall);
        ToggleMuteCommand = new Command(async () => await ToggleMuteAsync());
        ToggleVideoCommand = new Command(async () => await ToggleVideoAsync());
        OpenChatCommand = new Command(async () => await OpenChatAsync());
        CloseChatCommand = new Command(() => IsChatOpen = false);
        SendChatCommand = new Command(async () => await SendChatAsync(), () => !string.IsNullOrWhiteSpace(ChatInput));
        HangUpCommand = new Command(async () => await HangUpAsync());
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
    public string ProviderSubtitle { get; }
    public string MuteLabel { get; }
    public string UnmuteLabel { get; }
    public string VideoOnLabel { get; }
    public string VideoOffLabel { get; }
    public string ChatLabel { get; }
    public string HangUpLabel { get; }
    public string JoinPromptText { get; }
    public string ChatSendLabel { get; }
    public string ChatCloseLabel { get; }
    public string ChatEmptyText { get; }

    public ObservableCollection<TelehealthChatMessageViewModel> ChatMessages { get; }

    public bool IsChatOpen
    {
        get => _isChatOpen;
        set
        {
            if (_isChatOpen == value)
                return;
            _isChatOpen = value;
            OnPropertyChanged(nameof(IsChatOpen));
            if (_isChatOpen)
                StartChatPolling();
            else
                StopChatPolling();
        }
    }

    public string ChatInput
    {
        get => _chatInput;
        set
        {
            SetProperty(ref _chatInput, value);
            RaiseCanExecuteChanged(SendChatCommand);
        }
    }

    public ICommand CloseChatCommand { get; }
    public ICommand SendChatCommand { get; }

    public bool ShowVideoSection => _showVideoSection;
    public bool ShowSetupLayout => !InCall;
    public bool ShowConsultationLayout => InCall;

    public string ProviderDisplayName
    {
        get => _providerDisplayName;
        set => SetProperty(ref _providerDisplayName, value);
    }

    public string CallDurationText
    {
        get => _callDurationText;
        private set => SetProperty(ref _callDurationText, value);
    }

    public string ConnectionBadgeText
    {
        get => _connectionBadgeText;
        private set => SetProperty(ref _connectionBadgeText, value);
    }

    public string MuteButtonText => IsMuted ? UnmuteLabel : MuteLabel;
    public string VideoButtonText => IsVideoOff ? VideoOnLabel : VideoOffLabel;

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (_isMuted == value)
                return;
            _isMuted = value;
            OnPropertyChanged(nameof(IsMuted));
            OnPropertyChanged(nameof(MuteButtonText));
        }
    }

    public bool IsVideoOff
    {
        get => _isVideoOff;
        set
        {
            if (_isVideoOff == value)
                return;
            _isVideoOff = value;
            OnPropertyChanged(nameof(IsVideoOff));
            OnPropertyChanged(nameof(VideoButtonText));
        }
    }

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
            OnPropertyChanged(nameof(ShowSetupLayout));
            OnPropertyChanged(nameof(ShowConsultationLayout));
            OnPropertyChanged(nameof(ShowStartVideo));
            OnPropertyChanged(nameof(ShowEndVideo));
            RaiseCanExecuteChanged(StartVideoCommand, EndVideoCommand);

            if (_inCall)
                StartCallTimer();
            else
                StopCallTimer();
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
    public ICommand ToggleMuteCommand { get; }
    public ICommand ToggleVideoCommand { get; }
    public ICommand OpenChatCommand { get; }
    public ICommand HangUpCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (TryGetQueryGuid(query, "sessionId", out var id))
            _sessionId = id;
    }

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
            if (!string.IsNullOrWhiteSpace(j.ProviderDisplayName))
                ProviderDisplayName = j.ProviderDisplayName.Trim();

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
            RaiseCanExecuteChanged(StartVideoCommand);
        }
        catch (Exception)
        {
            ErrorMessage = T("CareTelehealthJoinFailed");
        }
        finally
        {
            IsBusy = false;
        }

        if (CanStartVideo && _showVideoSection && !InCall)
            await StartVideoAsync().ConfigureAwait(false);
    }

    private async Task StartVideoAsync()
    {
        if (!CanStartVideo)
            return;

        ConnectionBadgeText = T("ConsultationConnecting");
        IsBusy = true;
        try
        {
            var result = await _rtc.StartAsync(
                new TelehealthRtcJoinParameters(_joinAppId, _joinChannel, _fullToken, _joinUid),
                CancellationToken.None).ConfigureAwait(false);

            if (!result.Success)
            {
                await DisplayAlertSafeAsync(Title, result.ErrorMessage ?? T("CareTelehealthVideoStartFailed"), T("CommonOk"));
                return;
            }

            InCall = true;
            UpdateConnectionBadge();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ToggleMuteAsync()
    {
        if (!InCall)
            return;

        var next = !IsMuted;
        await _rtc.SetMicrophoneMutedAsync(next, CancellationToken.None).ConfigureAwait(false);
        IsMuted = next;
    }

    private async Task ToggleVideoAsync()
    {
        if (!InCall)
            return;

        var nextOff = !IsVideoOff;
        await _rtc.SetCameraEnabledAsync(!nextOff, CancellationToken.None).ConfigureAwait(false);
        IsVideoOff = nextOff;
    }

    private void OnRtcChannelJoined(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(UpdateConnectionBadge);

    private void OnRtcRemoteUserJoined(object? sender, int uid)
    {
        _remoteParticipantPresent = true;
        MainThread.BeginInvokeOnMainThread(UpdateConnectionBadge);
    }

    private void OnRtcRemoteUserLeft(object? sender, int uid)
    {
        _remoteParticipantPresent = false;
        MainThread.BeginInvokeOnMainThread(UpdateConnectionBadge);
    }

    private void UpdateConnectionBadge()
    {
        if (!InCall)
            return;

        ConnectionBadgeText = _remoteParticipantPresent
            ? T("ConsultationConnected")
            : T("ConsultationWaitingProvider");
    }

    private async Task EndVideoAsync()
    {
        IsBusy = true;
        try
        {
            await _rtc.StopAsync(CancellationToken.None).ConfigureAwait(false);
            InCall = false;
            _remoteParticipantPresent = false;
            IsMuted = false;
            IsVideoOff = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task HangUpAsync()
    {
        IsChatOpen = false;
        await EndVideoAsync().ConfigureAwait(false);
        await SafeShellNavigator.GoToAsync("..");
    }

    private async Task GoBackAsync()
    {
        await _rtc.StopAsync(CancellationToken.None).ConfigureAwait(false);
        InCall = false;
        await SafeShellNavigator.GoToAsync("..");
    }

    private void StartCallTimer()
    {
        _callStarted = DateTimeOffset.UtcNow;
        _callTimer?.Dispose();
        _callTimer = new System.Threading.Timer(_ =>
        {
            var elapsed = DateTimeOffset.UtcNow - _callStarted;
            MainThread.BeginInvokeOnMainThread(() =>
                CallDurationText = elapsed.ToString(@"mm\:ss", CultureInfo.InvariantCulture));
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void StopCallTimer()
    {
        _callTimer?.Dispose();
        _callTimer = null;
        CallDurationText = "00:00";
    }

    private async Task CopyTokenAsync()
    {
        if (string.IsNullOrEmpty(_fullToken))
        {
            await DisplayAlertSafeAsync(Title, T("CareTelehealthTokenUnset"), T("CommonOk"));
            return;
        }

        await Clipboard.Default.SetTextAsync(_fullToken);
        await DisplayAlertSafeAsync(Title, T("CareTelehealthCopied"), T("CommonOk"));
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
                await DisplayAlertSafeAsync(Title, response.ErrorMessage ?? T("CareTelehealthSmsFailed"), T("CommonOk"));
                return;
            }

            await DisplayAlertSafeAsync(Title, T("CareTelehealthSmsSent"), T("CommonOk"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Dispose()
    {
        _callTimer?.Dispose();
        StopChatPolling();
        _rtc.ChannelJoined -= OnRtcChannelJoined;
        _rtc.RemoteUserJoined -= OnRtcRemoteUserJoined;
        _rtc.RemoteUserLeft -= OnRtcRemoteUserLeft;
    }

    private async Task OpenChatAsync()
    {
        if (_sessionId == Guid.Empty)
            return;

        IsChatOpen = true;
        await RefreshChatAsync().ConfigureAwait(false);
    }

    private async Task SendChatAsync()
    {
        if (_sessionId == Guid.Empty || string.IsNullOrWhiteSpace(ChatInput))
            return;

        var text = ChatInput.Trim();
        ChatInput = string.Empty;
        var response = await _telehealth.SendChatMessageAsync(_sessionId, text, CancellationToken.None).ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            await DisplayAlertSafeAsync(Title, response.ErrorMessage ?? T("ConsultationChatSendFailed"), T("CommonOk"));
            return;
        }

        await RefreshChatAsync().ConfigureAwait(false);
    }

    private void StartChatPolling()
    {
        _chatPollTimer?.Dispose();
        _chatPollTimer = new System.Threading.Timer(async _ =>
        {
            try
            {
                await RefreshChatAsync().ConfigureAwait(false);
            }
            catch
            {
                // Polling is best-effort during the call.
            }
        }, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
    }

    private void StopChatPolling()
    {
        _chatPollTimer?.Dispose();
        _chatPollTimer = null;
    }

    private async Task RefreshChatAsync()
    {
        if (_sessionId == Guid.Empty)
            return;

        var response = await _telehealth.GetSessionChatAsync(_sessionId, 50, CancellationToken.None).ConfigureAwait(false);
        if (!response.IsSuccess || response.Data is null)
            return;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ChatMessages.Clear();
            foreach (var m in response.Data)
                ChatMessages.Add(m);
        });
    }
}
