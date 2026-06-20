using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

public sealed class LanguageOption : INotifyPropertyChanged
{
    private bool _isCurrent;

    public LanguageOption(string code, string native, string english, CultureInfo culture)
    {
        Code = code;
        Native = native;
        English = english;
        Culture = culture;
    }

    public string Code { get; }
    public string Native { get; }
    public string English { get; }
    public CultureInfo Culture { get; }

    public bool IsCurrent
    {
        get => _isCurrent;
        internal set
        {
            if (_isCurrent == value)
                return;
            _isCurrent = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// Welcome screen aligned with the React concept: gradient background, logo card, language picker, Entra sign-in entry points.
/// </summary>
public class LandingViewModel : BaseViewModel
{
    private const string LanguagePreferenceKey = "auth_language";

    private readonly EntraAuthOptions _entra;

    private string _languageCode = "en";
    private bool _isLanguageSheetOpen;

    public LandingViewModel(IOptions<EntraAuthOptions> entraOptions)
    {
        _entra = entraOptions?.Value ?? throw new ArgumentNullException(nameof(entraOptions));
        Title = T("AuthLandingPageTitle");
        Languages = new ObservableCollection<LanguageOption>(new[]
        {
            new LanguageOption("en", "English", "English", CultureInfo.GetCultureInfo("en-US")),
            new LanguageOption("fr", "Français", "French", CultureInfo.GetCultureInfo("fr")),
            new LanguageOption("ln", "Lingála", "Lingala", CultureInfo.GetCultureInfo("ln")),
            new LanguageOption("sw", "Kiswahili", "Swahili", CultureInfo.GetCultureInfo("sw")),
        });

        CreateAccountCommand = new Command(async () => await GoToRegisterOptionsAsync().ConfigureAwait(false));
        SignInCommand = new Command(async () => await SignInAsync().ConfigureAwait(false));
        OpenLanguageSheetCommand = new Command(() => IsLanguageSheetOpen = true);
        CloseLanguageSheetCommand = new Command(() => IsLanguageSheetOpen = false);
        SelectLanguageCommand = new Command<LanguageOption>(ApplyLanguage);

        RestoreLanguagePreference();
    }

    public ObservableCollection<LanguageOption> Languages { get; }

    public ICommand CreateAccountCommand { get; }
    public ICommand SignInCommand { get; }
    public ICommand OpenLanguageSheetCommand { get; }
    public ICommand CloseLanguageSheetCommand { get; }
    public ICommand SelectLanguageCommand { get; }

    public bool IsLanguageSheetOpen
    {
        get => _isLanguageSheetOpen;
        set => SetProperty(ref _isLanguageSheetOpen, value);
    }

    public string CurrentLanguageNative =>
        Languages.FirstOrDefault(l => l.Code == _languageCode)?.Native ?? "English";

    public string WelcomeTagline => T("AuthWelcomeTagline");

    public string FooterTagline => T("AuthHealthcareBarriers");

    public string SignInText => T("AuthSignIn");

    public string CreateAccountText => T("AuthCreateAccount");

    public string ChooseLanguageTitle => T("AuthChooseLanguage");

    private void RestoreLanguagePreference()
    {
        _languageCode = Preferences.Get(LanguagePreferenceKey, "en");
        ApplyUiCulture(ResolveCulture(_languageCode));
        SyncLanguageSelection();
        NotifyLocalizedProperties();
        OnPropertyChanged(nameof(CurrentLanguageNative));
    }

    private void ApplyLanguage(LanguageOption? option)
    {
        if (option is null)
            return;

        _languageCode = option.Code;
        Preferences.Set(LanguagePreferenceKey, option.Code);
        ApplyUiCulture(option.Culture);
        SyncLanguageSelection();
        NotifyLocalizedProperties();
        OnPropertyChanged(nameof(CurrentLanguageNative));
        IsLanguageSheetOpen = false;
    }

    private void SyncLanguageSelection()
    {
        foreach (var l in Languages)
            l.IsCurrent = l.Code == _languageCode;
    }

    private static void ApplyUiCulture(CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static CultureInfo ResolveCulture(string code) =>
        code switch
        {
            "fr" => CultureInfo.GetCultureInfo("fr"),
            "sw" => CultureInfo.GetCultureInfo("sw"),
            "ln" => CultureInfo.GetCultureInfo("ln"),
            _ => CultureInfo.GetCultureInfo("en-US"),
        };

    private void NotifyLocalizedProperties()
    {
        OnPropertyChanged(nameof(WelcomeTagline));
        OnPropertyChanged(nameof(FooterTagline));
        OnPropertyChanged(nameof(SignInText));
        OnPropertyChanged(nameof(CreateAccountText));
        OnPropertyChanged(nameof(ChooseLanguageTitle));
    }

    private async Task GoToRegisterOptionsAsync()
    {
        var url = _entra.ExternalSignUpUrl;
        if (!string.IsNullOrWhiteSpace(url))
            await Launcher.Default.OpenAsync(new Uri(url.Trim(), UriKind.Absolute)).ConfigureAwait(false);
        else
            await SafeShellNavigator.GoToAsync("RegisterOptionsPage");
    }

    private async Task SignInAsync()
    {
        await SafeShellNavigator.GoToAsync("SignInPage");
    }
}
