using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;

public sealed class TelehealthJoinViewModel : BaseViewModel
{
    private readonly IPatientTelehealthService _telehealth;
    private string _fullToken = string.Empty;
    private Guid _sessionId;
    private string _channel = string.Empty;
    private string _uid = string.Empty;
    private string _appId = string.Empty;
    private string _tokenPreview = string.Empty;
    private string _rtcStatus = string.Empty;
    private string? _errorMessage;

    public TelehealthJoinViewModel(IPatientTelehealthService telehealth)
    {
        _telehealth = telehealth ?? throw new ArgumentNullException(nameof(telehealth));
        Title = AppResources.T("CareTelehealthJoinTitle");
        ChannelLabel = AppResources.T("CareTelehealthChannel");
        UidLabel = AppResources.T("CareTelehealthUid");
        AppIdLabel = AppResources.T("CareTelehealthAppId");
        TokenLabel = AppResources.T("CareTelehealthToken");
        CopyTokenLabel = AppResources.T("CareTelehealthCopyToken");
        SmsLabel = AppResources.T("CareTelehealthSendSms");
        BackLabel = AppResources.T("CareTelehealthBack");

        RefreshCommand = new Command(async () => await LoadAsync());
        CopyTokenCommand = new Command(async () => await CopyTokenAsync());
        SmsCommand = new Command(async () => await SendSmsAsync());
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
    }

    public string ChannelLabel { get; }
    public string UidLabel { get; }
    public string AppIdLabel { get; }
    public string TokenLabel { get; }
    public string CopyTokenLabel { get; }
    public string SmsLabel { get; }
    public string BackLabel { get; }

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

    public ICommand RefreshCommand { get; }
    public ICommand CopyTokenCommand { get; }
    public ICommand SmsCommand { get; }
    public ICommand BackCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("sessionId", out var v) && v != null && Guid.TryParse(v.ToString(), out var id))
            _sessionId = id;
    }

    public async Task LoadAsync()
    {
        if (_sessionId == Guid.Empty)
        {
            ErrorMessage = AppResources.T("CareTelehealthJoinFailed");
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _telehealth.GetJoinInfoAsync(_sessionId, null, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? AppResources.T("CareTelehealthJoinFailed");
                return;
            }

            var j = response.Data;
            ChannelText = j.ChannelName;
            UidText = j.Uid.ToString();
            AppIdText = string.IsNullOrEmpty(j.AppId) ? "—" : j.AppId;
            TokenPreview = string.IsNullOrEmpty(j.RtcToken)
                ? AppResources.T("CareTelehealthTokenUnset")
                : j.RtcToken.Length <= 24
                    ? j.RtcToken
                    : $"{j.RtcToken[..24]}…";
            RtcStatusText = j.RtcConfigured
                ? AppResources.T("CareTelehealthRtcReady")
                : AppResources.T("CareTelehealthRtcNotConfigured");
            _fullToken = j.RtcToken ?? string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CopyTokenAsync()
    {
        if (string.IsNullOrEmpty(_fullToken))
        {
            await Shell.Current.DisplayAlertAsync(Title, AppResources.T("CareTelehealthTokenUnset"), "OK");
            return;
        }

        await Clipboard.Default.SetTextAsync(_fullToken);
        await Shell.Current.DisplayAlertAsync(Title, AppResources.T("CareTelehealthCopied"), "OK");
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
                await Shell.Current.DisplayAlertAsync(Title, response.ErrorMessage ?? AppResources.T("CareTelehealthSmsFailed"), "OK");
                return;
            }

            await Shell.Current.DisplayAlertAsync(Title, AppResources.T("CareTelehealthSmsSent"), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
