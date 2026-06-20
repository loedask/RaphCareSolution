using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Features.Home.Models;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Home.ViewModels;

/// <summary>Home dashboard aligned with concept <c>Home.tsx</c>.</summary>
public sealed class HomeViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private string _greeting = string.Empty;
    private string _userDisplayName = string.Empty;

    public HomeViewModel(IAuthService auth)
    {
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));

        Title = AppResources.T("HomePageTitle");

        SectionUpcoming = AppResources.T("HomeUpcoming");
        SectionQuickActions = AppResources.T("HomeQuickActions");
        SectionHealthSummary = AppResources.T("HomeHealthSummary");
        SectionConnectedDevices = AppResources.T("HomeConnectedDevicesTitle");
        SectionAiInsight = AppResources.T("HomeAiWellnessInsight");
        SeeAllText = AppResources.T("HomeSeeAll");

        NeedHelpNow = AppResources.T("HomeNeedHelpNow");
        RequestCallTitle = AppResources.T("HomeRequestCall");
        RequestCallSubtitle = AppResources.T("HomeConnectWithDoctor");

        UpcomingDoctorName = AppResources.T("HomeDemoDoctorName");
        UpcomingDoctorInitials = AppResources.T("HomeDemoDoctorInitials");
        UpcomingConsultationType = AppResources.T("HomeGeneralConsultation");
        UpcomingTimeLabel = AppResources.T("HomeDemoAppointmentTime");
        UpcomingModeLabel = AppResources.T("HomeVideo");
        JoinConsultationText = AppResources.T("HomeJoinConsultation");

        DailyHealthTipTitle = AppResources.T("HomeDailyHealthTip");
        WellnessMessage = AppResources.T("HomeWellnessMessage");
        AskAiAssistantText = AppResources.T("HomeAskAiAssistant");

        QuickActions = new ObservableCollection<HomeQuickActionItem>(BuildQuickActions());
        HealthMetrics = new ObservableCollection<HomeHealthMetricItem>(BuildHealthMetrics());
        ConnectedDevices = new ObservableCollection<HomeConnectedDeviceItem>(BuildConnectedDevices());

        OpenNotificationsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Notifications, AppResources.HomeHubNotifications));
        RequestCallCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.CareTelehealth, AppResources.T("HomeRequestCall")));
        SeeAllAppointmentsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, AppResources.HomeHubAppointments));
        OpenUpcomingAppointmentCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, AppResources.HomeHubAppointments));
        JoinConsultationCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.CareTelehealth, AppResources.T("HomeJoinConsultation")));
        SeeAllDevicesCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, AppResources.HomeHubDevices));
        SeeAllHealthCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, AppResources.T("HomeHealthSummary")));
        AskAiAssistantCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.AiAssistant, AppResources.HomeHubAiAssistant));

        RefreshGreeting();
    }

    public string Greeting
    {
        get => _greeting;
        private set => SetProperty(ref _greeting, value);
    }

    public string UserDisplayName
    {
        get => _userDisplayName;
        private set => SetProperty(ref _userDisplayName, value);
    }

    public string SectionUpcoming { get; }
    public string SectionQuickActions { get; }
    public string SectionHealthSummary { get; }
    public string SectionConnectedDevices { get; }
    public string SectionAiInsight { get; }
    public string SeeAllText { get; }

    public string NeedHelpNow { get; }
    public string RequestCallTitle { get; }
    public string RequestCallSubtitle { get; }

    public string UpcomingDoctorName { get; }
    public string UpcomingDoctorInitials { get; }
    public string UpcomingConsultationType { get; }
    public string UpcomingTimeLabel { get; }
    public string UpcomingModeLabel { get; }
    public string JoinConsultationText { get; }

    public string DailyHealthTipTitle { get; }
    public string WellnessMessage { get; }
    public string AskAiAssistantText { get; }

    public ObservableCollection<HomeQuickActionItem> QuickActions { get; }
    public ObservableCollection<HomeHealthMetricItem> HealthMetrics { get; }
    public ObservableCollection<HomeConnectedDeviceItem> ConnectedDevices { get; }

    public ICommand OpenNotificationsCommand { get; }
    public ICommand RequestCallCommand { get; }
    public ICommand SeeAllAppointmentsCommand { get; }
    public ICommand OpenUpcomingAppointmentCommand { get; }
    public ICommand JoinConsultationCommand { get; }
    public ICommand SeeAllDevicesCommand { get; }
    public ICommand SeeAllHealthCommand { get; }
    public ICommand AskAiAssistantCommand { get; }

    public void RefreshGreeting()
    {
        Greeting = DateTime.Now.Hour switch
        {
            >= 5 and < 12 => AppResources.T("HomeGreetingMorning"),
            >= 12 and < 17 => AppResources.T("HomeGreetingAfternoon"),
            _ => AppResources.T("HomeGreetingEvening"),
        };
    }

    public async Task RefreshAsync()
    {
        RefreshGreeting();

        var token = await _auth.GetAccessTokenAsync(CancellationToken.None).ConfigureAwait(false);
        var (name, _) = JwtClaimsReader.ReadDisplayClaims(token);
        var displayName = ResolveFirstName(name);
        await MainThread.InvokeOnMainThreadAsync(() => UserDisplayName = displayName);
    }

    private static string ResolveFirstName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return AppResources.T("HomeDefaultUserName");

        var first = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(first) ? AppResources.T("HomeDefaultUserName") : first;
    }

    private static HomeQuickActionItem[] BuildQuickActions() =>
    [
        new HomeQuickActionItem
        {
            Title = AppResources.T("HomeBookAppointment"),
            IconGlyph = "📅",
            IsAccent = true,
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.BookAppointment, AppResources.T("HomeBookAppointment"))),
        },
        new HomeQuickActionItem
        {
            Title = AppResources.T("HomeHealthRecordsQuick"),
            IconGlyph = "📋",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Records, AppResources.HomeHubHealthRecords)),
        },
        new HomeQuickActionItem
        {
            Title = AppResources.T("HomeInsurancePlanQuick"),
            IconGlyph = "🛡️",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Insurance, AppResources.HomeHubInsurance)),
        },
        new HomeQuickActionItem
        {
            Title = AppResources.T("HomeConnectedDevicesQuick"),
            IconGlyph = "⌚",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, AppResources.HomeHubDevices)),
        },
        new HomeQuickActionItem
        {
            Title = AppResources.T("HomeMentalHealthQuick"),
            IconGlyph = "🧠",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.MentalHealth, AppResources.HomeHubMentalHealth)),
        },
        new HomeQuickActionItem
        {
            Title = AppResources.T("HomeAiAssistantQuick"),
            IconGlyph = "✨",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.AiAssistant, AppResources.HomeHubAiAssistant)),
        },
    ];

    private static HomeHealthMetricItem[] BuildHealthMetrics() =>
    [
        CreateMetric(
            AppResources.T("HomeHeartRate"),
            "72",
            "bpm",
            "❤️",
            "+2",
            "↑",
            [40, 55, 35, 60, 50, 70, 45, 65, 55, 72]),
        CreateMetric(
            AppResources.T("HomeBloodPressure"),
            "120/80",
            "mmHg",
            "📈",
            "-1",
            "↓",
            [65, 58, 70, 55, 62, 68, 60, 72, 64, 66]),
    ];

    private static HomeHealthMetricItem CreateMetric(
        string label,
        string value,
        string unit,
        string icon,
        string trendDelta,
        string trendGlyph,
        int[] barPercents)
    {
        var sparkline = new ObservableCollection<HomeSparklineBar>();
        for (var i = 0; i < barPercents.Length; i++)
        {
            sparkline.Add(new HomeSparklineBar
            {
                Height = 36 * barPercents[i] / 100.0,
                IsLatest = i == barPercents.Length - 1,
            });
        }

        return new HomeHealthMetricItem
        {
            Label = label,
            Value = value,
            Unit = unit,
            Status = AppResources.T("HomeNormal"),
            TrendLabel = $"{trendDelta} {AppResources.T("HomeTrendVsLastWeek")}",
            TrendGlyph = trendGlyph,
            IconGlyph = icon,
            Sparkline = sparkline,
        };
    }

    private static HomeConnectedDeviceItem[] BuildConnectedDevices()
    {
        var devicesCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, AppResources.HomeHubDevices));

        return
        [
            new HomeConnectedDeviceItem
            {
                Name = AppResources.T("HomeBloodPressure"),
                Value = "120/80",
                Unit = "mmHg",
                Synced = AppResources.T("HomeSyncedMinutesAgo").Replace("{0}", "2"),
                IconGlyph = "📈",
                TapCommand = devicesCommand,
            },
            new HomeConnectedDeviceItem
            {
                Name = AppResources.T("HomeBloodGlucose"),
                Value = "95",
                Unit = "mg/dL",
                Synced = AppResources.T("HomeSyncedMinutesAgo").Replace("{0}", "15"),
                IconGlyph = "💧",
                TapCommand = devicesCommand,
            },
            new HomeConnectedDeviceItem
            {
                Name = AppResources.T("HomeHeartRate"),
                Value = "72",
                Unit = "bpm",
                Synced = AppResources.T("HomeSyncedLive"),
                IconGlyph = "❤️",
                TapCommand = devicesCommand,
            },
        ];
    }
}
