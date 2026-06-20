using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Features.Home.Models;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.ViewModels;

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

        Title = T("HomePageTitle");

        SectionUpcoming = T("HomeUpcoming");
        SectionQuickActions = T("HomeQuickActions");
        SectionHealthSummary = T("HomeHealthSummary");
        SectionConnectedDevices = T("HomeConnectedDevicesTitle");
        SectionAiInsight = T("HomeAiWellnessInsight");
        SeeAllText = T("HomeSeeAll");

        NeedHelpNow = T("HomeNeedHelpNow");
        RequestCallTitle = T("HomeRequestCall");
        RequestCallSubtitle = T("HomeConnectWithDoctor");

        UpcomingDoctorName = T("HomeDemoDoctorName");
        UpcomingDoctorInitials = T("HomeDemoDoctorInitials");
        UpcomingConsultationType = T("HomeGeneralConsultation");
        UpcomingTimeLabel = T("HomeDemoAppointmentTime");
        UpcomingModeLabel = T("HomeVideo");
        JoinConsultationText = T("HomeJoinConsultation");

        DailyHealthTipTitle = T("HomeDailyHealthTip");
        WellnessMessage = T("HomeWellnessMessage");
        AskAiAssistantText = T("HomeAskAiAssistant");

        QuickActions = new ObservableCollection<HomeQuickActionItem>(BuildQuickActions());
        HealthMetrics = new ObservableCollection<HomeHealthMetricItem>(BuildHealthMetrics());
        ConnectedDevices = new ObservableCollection<HomeConnectedDeviceItem>(BuildConnectedDevices());

        OpenNotificationsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Notifications, T("HomeHubNotifications")));
        RequestCallCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.CareTelehealth, T("HomeRequestCall")));
        SeeAllAppointmentsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, T("HomeHubAppointments")));
        OpenUpcomingAppointmentCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, T("HomeHubAppointments")));
        JoinConsultationCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.CareTelehealth, T("HomeJoinConsultation")));
        SeeAllDevicesCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, T("HomeHubDevices")));
        SeeAllHealthCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, T("HomeHealthSummary")));
        AskAiAssistantCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.AiAssistant, T("HomeHubAiAssistant")));

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
            >= 5 and < 12 => T("HomeGreetingMorning"),
            >= 12 and < 17 => T("HomeGreetingAfternoon"),
            _ => T("HomeGreetingEvening"),
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
            return T("HomeDefaultUserName");

        var first = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(first) ? T("HomeDefaultUserName") : first;
    }

    private static HomeQuickActionItem[] BuildQuickActions() =>
    [
        new HomeQuickActionItem
        {
            Title = T("HomeBookAppointment"),
            IconGlyph = "📅",
            IsAccent = true,
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.BookAppointment, T("HomeBookAppointment"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeHealthRecordsQuick"),
            IconGlyph = "📋",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Records, T("HomeHubHealthRecords"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeInsurancePlanQuick"),
            IconGlyph = "🛡️",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Insurance, T("HomeHubInsurance"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeConnectedDevicesQuick"),
            IconGlyph = "⌚",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, T("HomeHubDevices"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeMentalHealthQuick"),
            IconGlyph = "🧠",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.MentalHealth, T("HomeHubMentalHealth"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeAiAssistantQuick"),
            IconGlyph = "✨",
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.AiAssistant, T("HomeHubAiAssistant"))),
        },
    ];

    private static HomeHealthMetricItem[] BuildHealthMetrics() =>
    [
        CreateMetric(
            T("HomeHeartRate"),
            "72",
            "bpm",
            "❤️",
            "+2",
            "↑",
            [40, 55, 35, 60, 50, 70, 45, 65, 55, 72]),
        CreateMetric(
            T("HomeBloodPressure"),
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
            Status = T("HomeNormal"),
            TrendLabel = $"{trendDelta} {T("HomeTrendVsLastWeek")}",
            TrendGlyph = trendGlyph,
            IconGlyph = icon,
            Sparkline = sparkline,
        };
    }

    private static HomeConnectedDeviceItem[] BuildConnectedDevices()
    {
        var devicesCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, T("HomeHubDevices")));

        return
        [
            new HomeConnectedDeviceItem
            {
                Name = T("HomeBloodPressure"),
                Value = "120/80",
                Unit = "mmHg",
                Synced = T("HomeSyncedMinutesAgo").Replace("{0}", "2"),
                IconGlyph = "📈",
                TapCommand = devicesCommand,
            },
            new HomeConnectedDeviceItem
            {
                Name = T("HomeBloodGlucose"),
                Value = "95",
                Unit = "mg/dL",
                Synced = T("HomeSyncedMinutesAgo").Replace("{0}", "15"),
                IconGlyph = "💧",
                TapCommand = devicesCommand,
            },
            new HomeConnectedDeviceItem
            {
                Name = T("HomeHeartRate"),
                Value = "72",
                Unit = "bpm",
                Synced = T("HomeSyncedLive"),
                IconGlyph = "❤️",
                TapCommand = devicesCommand,
            },
        ];
    }
}
