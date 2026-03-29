using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Home.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        WelcomeLabel.Text = AppResources.HomeWelcome;
        SignedInLabel.Text = AppResources.HomeSignedIn;
        SectionCareLabel.Text = AppResources.HomeHubSectionCare;
        SectionMoreLabel.Text = AppResources.HomeHubSectionMore;
        SectionSamplesLabel.Text = AppResources.HomeHubSamples;
        HealthRecordsButton.Text = AppResources.HomeHubHealthRecords;
        AppointmentsButton.Text = AppResources.HomeHubAppointments;
        InsuranceButton.Text = AppResources.HomeHubInsurance;
        CareTelehealthButton.Text = AppResources.HomeHubCareTelehealth;
        DevicesButton.Text = AppResources.HomeHubDevices;
        BillingButton.Text = AppResources.HomeHubBilling;
        MentalHealthButton.Text = AppResources.HomeHubMentalHealth;
        FamilyMembersButton.Text = AppResources.HomeHubFamily;
        AiAssistantButton.Text = AppResources.HomeHubAiAssistant;
        NotificationsButton.Text = AppResources.HomeHubNotifications;
        SettingsButton.Text = AppResources.HomeHubSettings;
        OpenBlazorButton.Text = AppResources.OpenBlazorSample;
    }

    private async void OnHealthRecordsClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.Records, AppResources.HomeHubHealthRecords);

    private async void OnAppointmentsClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, AppResources.HomeHubAppointments);

    private async void OnInsuranceClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.Insurance, AppResources.HomeHubInsurance);

    private async void OnCareTelehealthClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.CareTelehealth, AppResources.HomeHubCareTelehealth);

    private async void OnDevicesClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.Devices, AppResources.HomeHubDevices);

    private async void OnBillingClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.Billing, AppResources.HomeHubBilling);

    private async void OnMentalHealthClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.MentalHealth, AppResources.HomeHubMentalHealth);

    private async void OnFamilyMembersClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.FamilyMembers, AppResources.HomeHubFamily);

    private async void OnAiAssistantClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.AiAssistant, AppResources.HomeHubAiAssistant);

    private async void OnNotificationsClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.Notifications, AppResources.HomeHubNotifications);

    private async void OnSettingsClicked(object? sender, EventArgs e) =>
        await AppNavigator.GoToFeatureAsync(AppNavigator.Settings, AppResources.HomeHubSettings);

    private async void OnOpenBlazorClicked(object? sender, EventArgs e) =>
        await SafeShellNavigator.GoToAsync(AppNavigator.BlazorHost);
}
