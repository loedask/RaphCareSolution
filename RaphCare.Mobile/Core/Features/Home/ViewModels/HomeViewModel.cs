using System.Collections.ObjectModel;
using System.Windows.Input;
using RaphCare.Mobile.Core.Features.Home.Models;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Home.ViewModels;

/// <summary>Home dashboard: greeting, hero, and concept-style quick-link cards (vertical 2 parity).</summary>
public sealed class HomeViewModel : BaseViewModel
{
    private string _greeting = string.Empty;

    public HomeViewModel()
    {
        Title = AppResources.T("HomePageTitle");
        HeroTitle = AppResources.T("HomeHeroTitle");
        HeroSubtitle = AppResources.T("HomeHeroSubtitle");
        Tagline = AppResources.T("HomeDashboardTagline");
        SectionCare = AppResources.HomeHubSectionCare;
        SectionMore = AppResources.HomeHubSectionMore;
        SectionSamples = AppResources.HomeHubSamples;
        OpenBlazorText = AppResources.OpenBlazorSample;

        CareItems = new ObservableCollection<HomeQuickLinkItem>(BuildCareItems());
        MoreItems = new ObservableCollection<HomeQuickLinkItem>(BuildMoreItems());

        OpenBlazorCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.BlazorHost));

        RefreshGreeting();
    }

    public string Greeting
    {
        get => _greeting;
        private set => SetProperty(ref _greeting, value);
    }

    public string Tagline { get; }
    public string HeroTitle { get; }
    public string HeroSubtitle { get; }
    public string SectionCare { get; }
    public string SectionMore { get; }
    public string SectionSamples { get; }
    public string OpenBlazorText { get; }

    public ObservableCollection<HomeQuickLinkItem> CareItems { get; }
    public ObservableCollection<HomeQuickLinkItem> MoreItems { get; }

    public ICommand OpenBlazorCommand { get; }

    public void RefreshGreeting()
    {
        Greeting = DateTime.Now.Hour switch
        {
            >= 5 and < 12 => AppResources.T("HomeGreetingMorning"),
            >= 12 and < 17 => AppResources.T("HomeGreetingAfternoon"),
            _ => AppResources.T("HomeGreetingEvening"),
        };
    }

    private static HomeQuickLinkItem[] BuildCareItems() =>
    [
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubHealthRecords,
            Subtitle = AppResources.T("HomeDashHintRecords"),
            IconGlyph = "📋",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.Records, AppResources.HomeHubHealthRecords)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubAppointments,
            Subtitle = AppResources.T("HomeDashHintAppointments"),
            IconGlyph = "📅",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, AppResources.HomeHubAppointments)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubInsurance,
            Subtitle = AppResources.T("HomeDashHintInsurance"),
            IconGlyph = "🏥",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.Insurance, AppResources.HomeHubInsurance)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubCareTelehealth,
            Subtitle = AppResources.T("HomeDashHintCareTelehealth"),
            IconGlyph = "💬",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.CareTelehealth, AppResources.HomeHubCareTelehealth)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubDevices,
            Subtitle = AppResources.T("HomeDashHintDevices"),
            IconGlyph = "⌚",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, AppResources.HomeHubDevices)),
        },
    ];

    private static HomeQuickLinkItem[] BuildMoreItems() =>
    [
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubBilling,
            Subtitle = AppResources.T("HomeDashHintBilling"),
            IconGlyph = "💳",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.Billing, AppResources.HomeHubBilling)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubMentalHealth,
            Subtitle = AppResources.T("HomeDashHintMentalHealth"),
            IconGlyph = "🧠",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.MentalHealth, AppResources.HomeHubMentalHealth)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubFamily,
            Subtitle = AppResources.T("HomeDashHintFamily"),
            IconGlyph = "👨‍👩‍👧",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.FamilyMembers, AppResources.HomeHubFamily)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubAiAssistant,
            Subtitle = AppResources.T("HomeDashHintAiAssistant"),
            IconGlyph = "✨",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.AiAssistant, AppResources.HomeHubAiAssistant)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubNotifications,
            Subtitle = AppResources.T("HomeDashHintNotifications"),
            IconGlyph = "🔔",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.Notifications, AppResources.HomeHubNotifications)),
        },
        new HomeQuickLinkItem
        {
            Title = AppResources.HomeHubSettings,
            Subtitle = AppResources.T("HomeDashHintSettings"),
            IconGlyph = "⚙",
            NavigateCommand = new Command(async () => await AppNavigator.GoToFeatureAsync(AppNavigator.Settings, AppResources.HomeHubSettings)),
        },
    ];
}
