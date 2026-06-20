using System.Windows.Input;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.MentalHealth.ViewModels;

public sealed class MentalHealthViewModel : BaseViewModel
{
    private readonly IPatientMentalHealthService _mentalHealth;
    private int? _selectedMoodIndex;
    private string? _errorMessage;
    private string _insightTitle = string.Empty;
    private string _insightBody = string.Empty;
    private string _medicalDisclaimer = string.Empty;

    public MentalHealthViewModel(IPatientMentalHealthService mentalHealth)
    {
        _mentalHealth = mentalHealth ?? throw new ArgumentNullException(nameof(mentalHealth));
        Title = T("MentalHealthTitle");

        SelectMoodCommand = new Command<string>(async s =>
        {
            if (!int.TryParse(s, out var index))
                return;
            await OnSelectMoodAsync(index);
        });
        BookTherapyCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.BookAppointment));
        SessionHistoryCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.Appointments));
    }

    public string HowAreYouLabel => T("MentalHealthHowAreYou");
    public string DailyCheckInHint => T("MentalHealthDailyCheckIn");
    public string MoodGreat => T("MentalHealthMoodGreat");
    public string MoodGood => T("MentalHealthMoodGood");
    public string MoodOkay => T("MentalHealthMoodOkay");
    public string MoodLow => T("MentalHealthMoodLow");
    public string BookTherapyTitle => T("MentalHealthBookTherapy");
    public string BookTherapySubtitle => T("MentalHealthConnectTherapist");
    public string SessionHistoryTitle => T("MentalHealthSessionHistory");
    public string SessionHistorySubtitle => T("MentalHealthSessionsThisMonth");

    public string InsightTitle
    {
        get => _insightTitle;
        set => SetProperty(ref _insightTitle, value);
    }

    public string InsightBody
    {
        get => _insightBody;
        set => SetProperty(ref _insightBody, value);
    }

    public string MedicalDisclaimer
    {
        get => _medicalDisclaimer;
        set => SetProperty(ref _medicalDisclaimer, value);
    }

    public int? SelectedMoodIndex
    {
        get => _selectedMoodIndex;
        set => SetProperty(ref _selectedMoodIndex, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>Mood buttons pass <c>CommandParameter</c> "0"–"3".</summary>
    public ICommand SelectMoodCommand { get; }
    public ICommand BookTherapyCommand { get; }
    public ICommand SessionHistoryCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _mentalHealth.GetContentAsync(CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("MentalHealthLoadFailed");
                return;
            }

            InsightTitle = response.Data.InsightTitle;
            InsightBody = response.Data.InsightBody;
            MedicalDisclaimer = response.Data.MedicalDisclaimer;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnSelectMoodAsync(int index)
    {
        if (index is < 0 or > 3)
            return;

        SelectedMoodIndex = index;
        var response = await _mentalHealth.LogMoodCheckInAsync(index, null, CancellationToken.None).ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(
                    T("MentalHealthTitle"),
                    response.ErrorMessage ?? T("MentalHealthMoodSaveFailed"),
                    "OK"));
        }
    }
}
