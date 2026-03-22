using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Features.Auth.Models;
using RaphCare.Mobile.Core.Shared.Configuration;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>In-app recording then multipart upload to <c>api/onboarding/voice</c> (after OTP-verified phone).</summary>
public class VoiceSubmitViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IVoiceOnboardingService _voice;
    private readonly OnboardingOptions _onboarding;
    private readonly IAudioManager _audioManager;

    private IAudioRecorder? _recorder;
    private string? _recordingPath;
    private IDispatcherTimer? _waveTimer;
    private IDispatcherTimer? _maxDurationTimer;

    private string _phoneE164 = string.Empty;
    private string _language = string.Empty;
    private string? _errorMessage;
    private string? _successTranscription;
    private bool _isRecording;
    private bool _isProcessing;
    private double _pulseScale = 1;

    public VoiceSubmitViewModel(
        IVoiceOnboardingService voice,
        IOptions<OnboardingOptions> onboarding,
        IAudioManager audioManager)
    {
        _voice = voice ?? throw new ArgumentNullException(nameof(voice));
        _onboarding = onboarding?.Value ?? throw new ArgumentNullException(nameof(onboarding));
        _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
        _language = string.IsNullOrWhiteSpace(_onboarding.DefaultVoiceLanguage) ? "en-ZA" : _onboarding.DefaultVoiceLanguage;

        Title = AppResources.T("VoiceSubmitTitle");
        Subtitle = AppResources.T("VoiceSubmitSubtitle");
        PhoneLabel = AppResources.T("VoiceSubmitPhoneLabel");
        LanguageLabel = AppResources.T("VoiceSubmitLanguageLabel");
        StartRecordingText = AppResources.T("VoiceRecordStart");
        StopRecordingText = AppResources.T("VoiceRecordStop");
        ListeningTitle = AppResources.T("VoiceRecordListeningTitle");
        ListeningHint = AppResources.T("VoiceRecordListeningHint");
        ProcessingTitle = AppResources.T("VoiceRecordProcessingTitle");
        ProcessingHint = AppResources.T("VoiceRecordProcessingHint");
        SuccessTitle = AppResources.T("VoiceSubmitSuccessTitle");
        ContinueHomeText = AppResources.T("VoiceSubmitContinueHome");

        for (var i = 0; i < 20; i++)
            WaveBars.Add(new WaveBarItem());

        StartRecordingCommand = new Command(async () => await StartRecordingAsync(), () => !IsBusy && !IsRecording && !IsProcessing && !ShowSuccess);
        StopRecordingCommand = new Command(async () => await StopRecordingAndSubmitAsync(), () => !IsBusy && IsRecording);
        ContinueHomeCommand = new Command(async () => await Shell.Current.GoToAsync("//HomePage").ConfigureAwait(false));
        BackCommand = new Command(async () => await GoBackAsync());
    }

    public string Subtitle { get; }
    public string PhoneLabel { get; }
    public string LanguageLabel { get; }
    public string StartRecordingText { get; }
    public string StopRecordingText { get; }
    public string ListeningTitle { get; }
    public string ListeningHint { get; }
    public string ProcessingTitle { get; }
    public string ProcessingHint { get; }
    public string SuccessTitle { get; }
    public string ContinueHomeText { get; }

    public ObservableCollection<WaveBarItem> WaveBars { get; } = new();

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
            OnPropertyChanged(nameof(ShowIntroChrome));
            (StartRecordingCommand as Command)?.ChangeCanExecute();
        }
    }

    public bool ShowSuccess => !string.IsNullOrEmpty(SuccessTranscription);

    public bool IsRecording
    {
        get => _isRecording;
        private set
        {
            if (_isRecording == value)
                return;
            _isRecording = value;
            OnPropertyChanged(nameof(IsRecording));
            OnPropertyChanged(nameof(ShowIntroChrome));
            (StartRecordingCommand as Command)?.ChangeCanExecute();
            (StopRecordingCommand as Command)?.ChangeCanExecute();
        }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        private set
        {
            if (_isProcessing == value)
                return;
            _isProcessing = value;
            OnPropertyChanged(nameof(IsProcessing));
            OnPropertyChanged(nameof(ShowIntroChrome));
            (StartRecordingCommand as Command)?.ChangeCanExecute();
        }
    }

    /// <summary>Intro fields (phone, language) visible only before recording/processing/success.</summary>
    public bool ShowIntroChrome => !IsRecording && !IsProcessing && !ShowSuccess;

    public double PulseScale
    {
        get => _pulseScale;
        private set => SetProperty(ref _pulseScale, value);
    }

    public ICommand StartRecordingCommand { get; }
    public ICommand StopRecordingCommand { get; }
    public ICommand ContinueHomeCommand { get; }
    public ICommand BackCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Phone", out var p) && p != null)
            PhoneE164 = p.ToString() ?? string.Empty;
    }

    /// <summary>Stops an in-flight recording without uploading; used when leaving the page.</summary>
    public async Task CancelAsync()
    {
        StopAnimationTimers();
        try
        {
            if (_recorder is { IsRecording: true })
                await _recorder.StopAsync().ConfigureAwait(false);
        }
        catch
        {
            // best-effort cleanup
        }

        _recorder = null;
        TryDeleteFile(_recordingPath);
        _recordingPath = null;
        IsRecording = false;
        IsProcessing = false;
    }

    private async Task GoBackAsync()
    {
        await CancelAsync().ConfigureAwait(false);
        await Shell.Current.GoToAsync("..").ConfigureAwait(false);
    }

    private static AudioRecorderOptions BuildRecorderStartOptions() =>
        new()
        {
            Encoding = Plugin.Maui.Audio.Encoding.Wav,
            Channels = ChannelType.Mono,
            SampleRate = 44100,
            BitDepth = BitDepth.Pcm16bit,
            ThrowIfNotSupported = false
        };

    private async Task StartRecordingAsync()
    {
        if (IsRecording || IsProcessing || ShowSuccess) return;

        ErrorMessage = null;
        SuccessTranscription = null;

        if (string.IsNullOrWhiteSpace(PhoneE164))
        {
            ErrorMessage = AppResources.T("RegisterPhoneInvalid");
            return;
        }

        var perm = await Permissions.RequestAsync<Permissions.Microphone>().ConfigureAwait(false);
        if (perm != PermissionStatus.Granted)
        {
            ErrorMessage = AppResources.T("VoiceRecordPermissionDenied");
            return;
        }

        var clinicId = _onboarding.VoiceRegistrationClinicId;
        if (clinicId is null || clinicId == Guid.Empty)
        {
            ErrorMessage = AppResources.T("VoiceSubmitNoClinic");
            return;
        }

        try
        {
            _recorder = _audioManager.CreateRecorder();
            if (!_recorder.CanRecordAudio)
            {
                ErrorMessage = AppResources.T("VoiceRecordNotAvailable");
                _recorder = null;
                return;
            }

            _recordingPath = Path.Combine(FileSystem.Current.CacheDirectory, $"raphcare_voice_{Guid.NewGuid():N}.wav");
            await _recorder.StartAsync(_recordingPath, BuildRecorderStartOptions()).ConfigureAwait(false);
            IsRecording = true;
            StartAnimationTimers();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _recorder = null;
            TryDeleteFile(_recordingPath);
            _recordingPath = null;
            IsRecording = false;
            StopAnimationTimers();
        }
    }

    private async Task StopRecordingAndSubmitAsync()
    {
        if (_recorder is null || !IsRecording) return;

        var clinicId = _onboarding.VoiceRegistrationClinicId;
        if (clinicId is null || clinicId == Guid.Empty)
        {
            ErrorMessage = AppResources.T("VoiceSubmitNoClinic");
            await CancelAsync().ConfigureAwait(false);
            return;
        }

        StopAnimationTimers();
        IsBusy = true;
        try
        {
            IAudioSource source;
            try
            {
                source = await _recorder.StopAsync().ConfigureAwait(false);
            }
            finally
            {
                _recorder = null;
            }

            IsRecording = false;

            if (source is not FileAudioSource fileSource)
            {
                ErrorMessage = AppResources.T("VoiceRecordStopFailed");
                TryDeleteFile(_recordingPath);
                _recordingPath = null;
                return;
            }

            _recordingPath = fileSource.GetFilePath();
            IsProcessing = true;

            await using var stream = File.OpenRead(_recordingPath);
            var fileName = Path.GetFileName(_recordingPath);
            var result = await _voice.SubmitVoiceAsync(
                stream,
                fileName,
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
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsProcessing = false;
            IsBusy = false;
            TryDeleteFile(_recordingPath);
            _recordingPath = null;
            (StartRecordingCommand as Command)?.ChangeCanExecute();
        }
    }

    private void StartAnimationTimers()
    {
        StopAnimationTimers();
        var d = Application.Current?.Dispatcher;
        if (d is null) return;

        var phase = 0d;
        _waveTimer = d.CreateTimer();
        _waveTimer.Interval = TimeSpan.FromMilliseconds(110);
        _waveTimer.Tick += (_, _) =>
        {
            phase += 0.18;
            PulseScale = 1 + 0.1 * Math.Sin(phase);
            foreach (var bar in WaveBars)
                bar.Height = 8 + Random.Shared.NextDouble() * 28;
        };
        _waveTimer.Start();

        _maxDurationTimer = d.CreateTimer();
        _maxDurationTimer.Interval = TimeSpan.FromSeconds(120);
        _maxDurationTimer.Tick += async (_, _) =>
        {
            _maxDurationTimer?.Stop();
            if (IsRecording)
                await StopRecordingAndSubmitAsync().ConfigureAwait(true);
        };
        _maxDurationTimer.Start();
    }

    private void StopAnimationTimers()
    {
        if (_waveTimer is not null)
        {
            _waveTimer.Stop();
            _waveTimer = null;
        }

        if (_maxDurationTimer is not null)
        {
            _maxDurationTimer.Stop();
            _maxDurationTimer = null;
        }

        PulseScale = 1;
    }

    private static void TryDeleteFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignore
        }
    }
}
