using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.Devices;
using RaphCare.Client.Models.MentalHealth;
using RaphCare.Mobile.Core.Common.Home;
using RaphCare.Mobile.Core.Common.Icons;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Core.Features.Home.Models;
using RaphCare.Mobile.Core.Features.Notifications.Services;
using RaphCare.Mobile.Core.Features.Settings.Services;

namespace RaphCare.Mobile.Core.Features.Home.ViewModels;

/// <summary>Home dashboard: live appointments, devices, vitals, and mood-based wellness tip.</summary>
public sealed class HomeViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IPatientPushRegistrationService _pushRegistration;
    private readonly IAppointmentService _appointments;
    private readonly IPatientDevicesService _patientDevices;
    private readonly IPatientMentalHealthService _mentalHealth;

    private string _greeting = string.Empty;
    private string _userDisplayName = string.Empty;
    private string _upcomingDoctorName = string.Empty;
    private string _upcomingDoctorInitials = string.Empty;
    private string _upcomingConsultationType = string.Empty;
    private string _upcomingTimeLabel = string.Empty;
    private string _upcomingModeLabel = string.Empty;
    private string _dailyHealthTipTitle = string.Empty;
    private string _wellnessMessage = string.Empty;
    private bool _hasUpcomingAppointment;
    private bool _showUpcomingEmpty;
    private bool _showJoinConsultation;
    private bool _hasHealthMetrics;
    private bool _showHealthEmpty;
    private bool _hasConnectedDevices;
    private bool _showDevicesEmpty;
    private bool _hasMoodInsight;
    private bool _showMoodEmpty;
    private Guid? _upcomingAppointmentId;

    public HomeViewModel(
        IAuthService auth,
        IPatientPushRegistrationService pushRegistration,
        IAppointmentService appointments,
        IPatientDevicesService patientDevices,
        IPatientMentalHealthService mentalHealth)
    {
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        _pushRegistration = pushRegistration ?? throw new ArgumentNullException(nameof(pushRegistration));
        _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
        _patientDevices = patientDevices ?? throw new ArgumentNullException(nameof(patientDevices));
        _mentalHealth = mentalHealth ?? throw new ArgumentNullException(nameof(mentalHealth));

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

        JoinConsultationText = T("HomeJoinConsultation");
        UpcomingEmptyText = T("HomeNoUpcoming");
        HealthEmptyText = T("HomeNoHealthReadings");
        DevicesEmptyText = T("HomeNoConnectedDevices");
        BookFromHomeText = T("HomeBookFromEmpty");
        LogMoodCheckInText = T("HomeLogMoodCheckIn");

        DailyHealthTipTitle = T("HomeDailyHealthTip");
        WellnessMessage = T("HomeWellnessNoMood");
        AskAiAssistantText = T("HomeAskAiAssistant");
        ShowMoodEmpty = true;

        QuickActions = new ObservableCollection<HomeQuickActionItem>(BuildQuickActions());
        HealthMetrics = new ObservableCollection<HomeHealthMetricItem>();
        ConnectedDevices = new ObservableCollection<HomeConnectedDeviceItem>();
        ShowUpcomingEmpty = true;
        ShowHealthEmpty = true;
        ShowDevicesEmpty = true;

        OpenNotificationsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Notifications, T("HomeHubNotifications")));
        RequestCallCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.RequestCall, T("HomeRequestCall")));
        SeeAllAppointmentsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, T("HomeHubAppointments")));
        OpenUpcomingAppointmentCommand = new Command(async () => await OpenUpcomingAsync());
        JoinConsultationCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.CareTelehealth, T("HomeJoinConsultation")));
        BookFromHomeCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.BookAppointment, T("HomeBookAppointment")));
        SeeAllDevicesCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, T("HomeHubDevices")));
        SeeAllHealthCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, T("HomeHealthSummary")));
        AskAiAssistantCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.AiAssistant, T("HomeHubAiAssistant")));
        OpenMentalHealthCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.MentalHealth, T("HomeHubMentalHealth")));

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

    public string UpcomingDoctorName
    {
        get => _upcomingDoctorName;
        private set => SetProperty(ref _upcomingDoctorName, value);
    }

    public string UpcomingDoctorInitials
    {
        get => _upcomingDoctorInitials;
        private set => SetProperty(ref _upcomingDoctorInitials, value);
    }

    public string UpcomingConsultationType
    {
        get => _upcomingConsultationType;
        private set => SetProperty(ref _upcomingConsultationType, value);
    }

    public string UpcomingTimeLabel
    {
        get => _upcomingTimeLabel;
        private set => SetProperty(ref _upcomingTimeLabel, value);
    }

    public string UpcomingModeLabel
    {
        get => _upcomingModeLabel;
        private set => SetProperty(ref _upcomingModeLabel, value);
    }

    public string JoinConsultationText { get; }
    public string UpcomingEmptyText { get; }
    public string HealthEmptyText { get; }
    public string DevicesEmptyText { get; }
    public string BookFromHomeText { get; }
    public string LogMoodCheckInText { get; }

    public bool HasUpcomingAppointment
    {
        get => _hasUpcomingAppointment;
        private set => SetProperty(ref _hasUpcomingAppointment, value);
    }

    public bool ShowUpcomingEmpty
    {
        get => _showUpcomingEmpty;
        private set => SetProperty(ref _showUpcomingEmpty, value);
    }

    public bool ShowJoinConsultation
    {
        get => _showJoinConsultation;
        private set => SetProperty(ref _showJoinConsultation, value);
    }

    public bool HasHealthMetrics
    {
        get => _hasHealthMetrics;
        private set => SetProperty(ref _hasHealthMetrics, value);
    }

    public bool ShowHealthEmpty
    {
        get => _showHealthEmpty;
        private set => SetProperty(ref _showHealthEmpty, value);
    }

    public bool HasConnectedDevices
    {
        get => _hasConnectedDevices;
        private set => SetProperty(ref _hasConnectedDevices, value);
    }

    public bool ShowDevicesEmpty
    {
        get => _showDevicesEmpty;
        private set => SetProperty(ref _showDevicesEmpty, value);
    }

    public bool HasMoodInsight
    {
        get => _hasMoodInsight;
        private set => SetProperty(ref _hasMoodInsight, value);
    }

    public bool ShowMoodEmpty
    {
        get => _showMoodEmpty;
        private set => SetProperty(ref _showMoodEmpty, value);
    }

    public string DailyHealthTipTitle
    {
        get => _dailyHealthTipTitle;
        private set => SetProperty(ref _dailyHealthTipTitle, value);
    }

    public string WellnessMessage
    {
        get => _wellnessMessage;
        private set => SetProperty(ref _wellnessMessage, value);
    }

    public string AskAiAssistantText { get; }

    public ObservableCollection<HomeQuickActionItem> QuickActions { get; }
    public ObservableCollection<HomeHealthMetricItem> HealthMetrics { get; }
    public ObservableCollection<HomeConnectedDeviceItem> ConnectedDevices { get; }

    public ICommand OpenNotificationsCommand { get; }
    public ICommand RequestCallCommand { get; }
    public ICommand SeeAllAppointmentsCommand { get; }
    public ICommand OpenUpcomingAppointmentCommand { get; }
    public ICommand JoinConsultationCommand { get; }
    public ICommand BookFromHomeCommand { get; }
    public ICommand SeeAllDevicesCommand { get; }
    public ICommand SeeAllHealthCommand { get; }
    public ICommand AskAiAssistantCommand { get; }
    public ICommand OpenMentalHealthCommand { get; }

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

        try
        {
            await _pushRegistration.RegisterCurrentDeviceAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // Push registration is best-effort for demos; ignore token/API failures here.
        }

        var appointmentsTask = _appointments.GetMyAppointmentsAsync(1, 50, CancellationToken.None);
        var devicesTask = _patientDevices.GetMyDevicesAsync(CancellationToken.None);
        var readingsTask = _patientDevices.GetMyLatestReadingsAsync(CancellationToken.None);
        var moodsTask = _mentalHealth.GetMyMoodCheckInsAsync(14, CancellationToken.None);
        await Task.WhenAll(appointmentsTask, devicesTask, readingsTask, moodsTask).ConfigureAwait(false);

        var appointmentsResponse = await appointmentsTask.ConfigureAwait(false);
        var devicesResponse = await devicesTask.ConfigureAwait(false);
        var readingsResponse = await readingsTask.ConfigureAwait(false);
        var moodsResponse = await moodsTask.ConfigureAwait(false);

        var nowUtc = DateTime.UtcNow;
        AppointmentViewModel? next = null;
        if (appointmentsResponse.IsSuccess && appointmentsResponse.Data?.Items is { } items)
        {
            next = HomeUpcomingAppointmentRules.PickNextUpcoming(
                items,
                nowUtc,
                a => a.ScheduledStart.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(a.ScheduledStart, DateTimeKind.Utc)
                    : a.ScheduledStart.ToUniversalTime(),
                a => a.ScheduledEnd.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(a.ScheduledEnd, DateTimeKind.Utc)
                    : a.ScheduledEnd.ToUniversalTime(),
                a => a.Status);
        }

        var culture = CultureInfo.CurrentCulture;
        var metrics = BuildHealthMetricsFromReadings(
            readingsResponse.IsSuccess ? readingsResponse.Data : null);
        var devices = BuildConnectedDevicesFromApi(
            devicesResponse.IsSuccess ? devicesResponse.Data : null);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ApplyUpcoming(next, culture);
            HealthMetrics.Clear();
            foreach (var m in metrics)
                HealthMetrics.Add(m);
            HasHealthMetrics = HealthMetrics.Count > 0;
            ShowHealthEmpty = !HasHealthMetrics;

            ConnectedDevices.Clear();
            foreach (var d in devices)
                ConnectedDevices.Add(d);
            HasConnectedDevices = ConnectedDevices.Count > 0;
            ShowDevicesEmpty = !HasConnectedDevices;

            ApplyWellnessInsight(moodsResponse.IsSuccess ? moodsResponse.Data : null);
        });
    }

    private void ApplyWellnessInsight(IReadOnlyList<PatientMoodCheckInViewModel>? moods)
    {
        var scores = moods?.Select(m => m.MoodScore) ?? Enumerable.Empty<int>();
        var kind = HomeWellnessInsightRules.Classify(scores);
        if (kind == HomeWellnessInsightRules.Kind.None)
        {
            DailyHealthTipTitle = T("HomeDailyHealthTip");
            WellnessMessage = T("HomeWellnessNoMood");
            HasMoodInsight = false;
            ShowMoodEmpty = true;
            return;
        }

        DailyHealthTipTitle = T("HomeWellnessTitleFromMood");
        WellnessMessage = kind switch
        {
            HomeWellnessInsightRules.Kind.Positive => T("HomeWellnessPositive"),
            HomeWellnessInsightRules.Kind.Steady => T("HomeWellnessSteady"),
            HomeWellnessInsightRules.Kind.Mixed => T("HomeWellnessMixed"),
            HomeWellnessInsightRules.Kind.Low => T("HomeWellnessLow"),
            _ => T("HomeWellnessNoMood"),
        };
        HasMoodInsight = true;
        ShowMoodEmpty = false;
    }

    private async Task OpenUpcomingAsync()
    {
        if (_upcomingAppointmentId is { } id)
        {
            await SafeShellNavigator.GoToAsync($"{AppNavigator.AppointmentDetail}?appointmentId={id}");
            return;
        }

        await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, T("HomeHubAppointments"));
    }

    private void ApplyUpcoming(AppointmentViewModel? next, CultureInfo culture)
    {
        if (next is null)
        {
            _upcomingAppointmentId = null;
            HasUpcomingAppointment = false;
            ShowUpcomingEmpty = true;
            ShowJoinConsultation = false;
            UpcomingDoctorName = string.Empty;
            UpcomingDoctorInitials = string.Empty;
            UpcomingConsultationType = string.Empty;
            UpcomingTimeLabel = string.Empty;
            UpcomingModeLabel = string.Empty;
            return;
        }

        _upcomingAppointmentId = next.Id;
        var title = !string.IsNullOrWhiteSpace(next.Reason)
            ? next.Reason!.Trim()
            : (!string.IsNullOrWhiteSpace(next.Type) ? next.Type.Trim() : T("HomeAppointmentFallback"));
        var subtitle = !string.IsNullOrWhiteSpace(next.Reason) && !string.IsNullOrWhiteSpace(next.Type)
            ? next.Type.Trim()
            : (string.IsNullOrWhiteSpace(next.Status) ? T("HomeGeneralConsultation") : next.Status.Trim());

        var startLocal = next.ScheduledStart.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(next.ScheduledStart, DateTimeKind.Utc).ToLocalTime()
            : next.ScheduledStart.ToLocalTime();

        UpcomingDoctorName = title;
        UpcomingDoctorInitials = HomeUpcomingAppointmentRules.InitialsFromLabel(title);
        UpcomingConsultationType = subtitle;
        UpcomingTimeLabel = startLocal.ToString("g", culture);
        var isVideo = HomeUpcomingAppointmentRules.LooksLikeVideo(next.Type);
        UpcomingModeLabel = isVideo ? T("HomeVideo") : T("HomeInPerson");
        ShowJoinConsultation = isVideo;
        HasUpcomingAppointment = true;
        ShowUpcomingEmpty = false;
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
            IconSource = MonochromeIconKeys.CalendarOnAccent,
            IsAccent = true,
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.BookAppointment, T("HomeBookAppointment"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeHealthRecordsQuick"),
            IconSource = MonochromeIconKeys.ClipboardOnAccent,
            IsAccent = true,
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Records, T("HomeHubHealthRecords"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeInsurancePlanQuick"),
            IconSource = MonochromeIconKeys.ShieldOnAccent,
            IsAccent = true,
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Insurance, T("HomeHubInsurance"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeConnectedDevicesQuick"),
            IconSource = MonochromeIconKeys.WatchOnAccent,
            IsAccent = true,
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, T("HomeHubDevices"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeMentalHealthQuick"),
            IconSource = MonochromeIconKeys.BrainOnAccent,
            IsAccent = true,
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.MentalHealth, T("HomeHubMentalHealth"))),
        },
        new HomeQuickActionItem
        {
            Title = T("HomeAiAssistantQuick"),
            IconSource = MonochromeIconKeys.SparklesOnAccent,
            IsAccent = true,
            TapCommand = new Command(async () =>
                await AppNavigator.GoToFeatureAsync(AppNavigator.AiAssistant, T("HomeHubAiAssistant"))),
        },
    ];

    private static List<HomeHealthMetricItem> BuildHealthMetricsFromReadings(PatientLatestReadingsViewModel? readings)
    {
        var list = new List<HomeHealthMetricItem>();
        if (readings is null)
            return list;

        if (readings.HeartRateBpm is { } hr)
        {
            list.Add(CreateMetric(
                T("HomeHeartRate"),
                Math.Round(hr, MidpointRounding.AwayFromZero).ToString(CultureInfo.CurrentCulture),
                "bpm",
                MonochromeIconKeys.Heart,
                Color.FromArgb("#FFE4E8"),
                readings.HeartRateRecordedAt));
        }

        if (readings.SpO2Percent is { } spo2)
        {
            list.Add(CreateMetric(
                T("HomeSpO2"),
                Math.Round(spo2, 1, MidpointRounding.AwayFromZero).ToString(CultureInfo.CurrentCulture),
                "%",
                MonochromeIconKeys.Activity,
                Color.FromArgb("#D7F4F1"),
                readings.SpO2RecordedAt));
        }

        return list;
    }

    private static HomeHealthMetricItem CreateMetric(
        string label,
        string value,
        string unit,
        string iconSource,
        Color iconWellColor,
        DateTime? recordedAt)
    {
        var when = recordedAt.HasValue
            ? FormatRelativeOrLocal(recordedAt.Value)
            : T("HomeFromYourWatch");

        return new HomeHealthMetricItem
        {
            Label = label,
            Value = value,
            Unit = unit,
            Status = T("HomeSyncedStatus"),
            TrendLabel = when,
            TrendGlyph = string.Empty,
            IconSource = iconSource,
            IconWellColor = iconWellColor,
            Sparkline = new ObservableCollection<HomeSparklineBar>(),
        };
    }

    private static List<HomeConnectedDeviceItem> BuildConnectedDevicesFromApi(
        IReadOnlyList<PatientDeviceListItemViewModel>? devices)
    {
        var devicesCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, T("HomeHubDevices")));

        if (devices is null || devices.Count == 0)
            return [];

        return devices.Select(d =>
        {
            var name = string.IsNullOrWhiteSpace(d.Model) ? T("HomeConnectedDevicesTitle") : d.Model.Trim();
            return new HomeConnectedDeviceItem
            {
                Name = name,
                Value = string.IsNullOrWhiteSpace(d.SerialNumber) ? "-" : d.SerialNumber.Trim(),
                Unit = T("HomeSerialShort"),
                Synced = T("HomeAssignedOn").Replace("{0}", FormatRelativeOrLocal(d.AssignedAt)),
                IconSource = MonochromeIconKeys.Watch,
                IconWellColor = Color.FromArgb("#E3EEFF"),
                TapCommand = devicesCommand,
            };
        }).ToList();
    }

    private static string FormatRelativeOrLocal(DateTime when)
    {
        var utc = when.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(when, DateTimeKind.Utc)
            : when.ToUniversalTime();
        var local = utc.ToLocalTime();
        var age = DateTime.UtcNow - utc;
        if (age.TotalMinutes is >= 0 and < 60)
            return T("HomeSyncedMinutesAgo").Replace("{0}", Math.Max(1, (int)age.TotalMinutes).ToString(CultureInfo.InvariantCulture));
        return local.ToString("g", CultureInfo.CurrentCulture);
    }
}
