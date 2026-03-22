using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Shared.Configuration;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Multipart voice upload to <c>api/onboarding/voice</c> after OTP-verified phone.</summary>
public class VoiceSubmitViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IVoiceOnboardingService _voice;
    private readonly OnboardingOptions _onboarding;

    private string _phoneE164 = string.Empty;
    private string _language = string.Empty;
    private string? _pickedFileName;
    private string? _errorMessage;
    private string? _successTranscription;

    public VoiceSubmitViewModel(IVoiceOnboardingService voice, IOptions<OnboardingOptions> onboarding)
    {
        _voice = voice ?? throw new ArgumentNullException(nameof(voice));
        _onboarding = onboarding?.Value ?? throw new ArgumentNullException(nameof(onboarding));
        _language = string.IsNullOrWhiteSpace(_onboarding.DefaultVoiceLanguage) ? "en-ZA" : _onboarding.DefaultVoiceLanguage;
        Title = AppResources.T("VoiceSubmitTitle");
        Subtitle = AppResources.T("VoiceSubmitSubtitle");
        PhoneLabel = AppResources.T("VoiceSubmitPhoneLabel");
        LanguageLabel = AppResources.T("VoiceSubmitLanguageLabel");
        PickFileText = AppResources.T("VoiceSubmitPickFile");
        SubmitText = AppResources.T("VoiceSubmitSubmit");
        SuccessTitle = AppResources.T("VoiceSubmitSuccessTitle");
        ContinueHomeText = AppResources.T("VoiceSubmitContinueHome");
        PickFileCommand = new Command(async () => await PickFileAsync(), () => !IsBusy);
        SubmitCommand = new Command(async () => await SubmitAsync(), () => !IsBusy);
        ContinueHomeCommand = new Command(async () => await Shell.Current.GoToAsync("//HomePage").ConfigureAwait(false));
        BackCommand = new Command(async () => await Shell.Current.GoToAsync("..").ConfigureAwait(false));
    }

    public string Subtitle { get; }
    public string PhoneLabel { get; }
    public string LanguageLabel { get; }
    public string PickFileText { get; }
    public string SubmitText { get; }
    public string SuccessTitle { get; }
    public string ContinueHomeText { get; }

    public string PhoneE164
    {
        get => _phoneE164;
        private set => SetProperty(ref _phoneE164, value);
    }

    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value ?? string.Empty);
    }

    public string? PickedFileName
    {
        get => _pickedFileName;
        set => SetProperty(ref _pickedFileName, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string? SuccessTranscription
    {
        get => _successTranscription;
        set
        {
            if (EqualityComparer<string?>.Default.Equals(_successTranscription, value))
                return;
            _successTranscription = value;
            OnPropertyChanged(nameof(SuccessTranscription));
            OnPropertyChanged(nameof(ShowSuccess));
        }
    }

    public bool ShowSuccess => !string.IsNullOrEmpty(SuccessTranscription);

    public ICommand PickFileCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand ContinueHomeCommand { get; }
    public ICommand BackCommand { get; }

    private FileResult? _picked;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Phone", out var p) && p != null)
            PhoneE164 = p.ToString() ?? string.Empty;
    }

    private async Task PickFileAsync()
    {
        ErrorMessage = null;
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = AppResources.T("VoiceSubmitPickTitle"),
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.audio", "public.mp3", "com.microsoft.waveform-audio" } },
                    { DevicePlatform.Android, new[] { "audio/*" } },
                    { DevicePlatform.WinUI, new[] { ".wav", ".mp3", ".m4a", ".ogg" } },
                    { DevicePlatform.MacCatalyst, new[] { "wav", "mp3", "m4a", "public.audio" } },
                }),
            }).ConfigureAwait(false);

            if (file == null) return;
            _picked = file;
            PickedFileName = file.FileName;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task SubmitAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        SuccessTranscription = null;

        if (_picked == null)
        {
            ErrorMessage = AppResources.T("VoiceSubmitNoFile");
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneE164))
        {
            ErrorMessage = AppResources.T("RegisterPhoneInvalid");
            return;
        }

        var clinicId = _onboarding.VoiceRegistrationClinicId;
        if (clinicId is null || clinicId == Guid.Empty)
        {
            ErrorMessage = AppResources.T("VoiceSubmitNoClinic");
            return;
        }

        IsBusy = true;
        try
        {
            using var stream = await _picked.OpenReadAsync().ConfigureAwait(false);
            var result = await _voice.SubmitVoiceAsync(
                stream,
                _picked.FileName,
                Language.Trim(),
                PhoneE164.Trim(),
                clinicId.Value,
                CancellationToken.None).ConfigureAwait(false);

            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.ErrorMessage ?? AppResources.T("VoiceSubmitFailed");
                return;
            }

            SuccessTranscription = result.Data.Transcription;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
