using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.MentalHealth;
using RaphCare.Mobile.Core.Common.Home;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.MentalHealth.ViewModels;

public sealed class MentalHealthViewModel : BaseViewModel
{
    private readonly IPatientMentalHealthService _mentalHealth;
    private readonly IClinicIdProvider _clinicIdProvider;
    private int? _selectedMoodIndex;
    private string? _errorMessage;
    private string _insightTitle = string.Empty;
    private string _insightBody = string.Empty;
    private string _medicalDisclaimer = string.Empty;
    private bool _showAssessmentForm;
    private string _assessmentTitle = string.Empty;
    private string _assessmentInstructions = string.Empty;
    private string _activeAssessmentType = string.Empty;
    private bool _submittingAssessment;
    private string? _assessmentMessage;

    public MentalHealthViewModel(
        IPatientMentalHealthService mentalHealth,
        IClinicIdProvider clinicIdProvider)
    {
        _mentalHealth = mentalHealth ?? throw new ArgumentNullException(nameof(mentalHealth));
        _clinicIdProvider = clinicIdProvider ?? throw new ArgumentNullException(nameof(clinicIdProvider));
        Title = T("MentalHealthTitle");

        HowAreYouLabel = T("MentalHealthHowAreYou");
        DailyCheckInHint = T("MentalHealthDailyCheckIn");
        MoodGreat = T("MentalHealthMoodGreat");
        MoodGood = T("MentalHealthMoodGood");
        MoodOkay = T("MentalHealthMoodOkay");
        MoodLow = T("MentalHealthMoodLow");
        BookTherapyTitle = T("MentalHealthBookTherapy");
        BookTherapySubtitle = T("MentalHealthConnectTherapist");
        SessionHistoryTitle = T("MentalHealthSessionHistory");
        SessionHistorySubtitle = T("MentalHealthSessionsThisMonth");
        SelfAssessmentTitle = T("MentalHealthSelfAssessmentTitle");
        SelfAssessmentHint = T("MentalHealthSelfAssessmentHint");
        StartPhq9Label = T("MentalHealthStartPhq9");
        StartGad7Label = T("MentalHealthStartGad7");
        SubmitAssessmentLabel = T("MentalHealthSubmitAssessment");
        CancelAssessmentLabel = T("MentalHealthCancelAssessment");
        SelectAnswerLabel = T("MentalHealthSelectAnswer");

        SelectMoodCommand = new Command<string>(async s =>
        {
            if (!int.TryParse(s, out var index))
                return;
            await OnSelectMoodAsync(index);
        });
        BookTherapyCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.BookAppointment));
        SessionHistoryCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.Appointments));
        StartPhq9Command = new Command(async () => await BeginAssessmentAsync("PHQ-9"));
        StartGad7Command = new Command(async () => await BeginAssessmentAsync("GAD-7"));
        CancelAssessmentCommand = new Command(CancelAssessment);
        SubmitAssessmentCommand = new Command(async () => await SubmitAssessmentAsync(), () => CanSubmitAssessment);
    }

    public string HowAreYouLabel { get; }
    public string DailyCheckInHint { get; }
    public string MoodGreat { get; }
    public string MoodGood { get; }
    public string MoodOkay { get; }
    public string MoodLow { get; }
    public string BookTherapyTitle { get; }
    public string BookTherapySubtitle { get; }
    public string SessionHistoryTitle { get; }
    public string SessionHistorySubtitle { get; }
    public string SelfAssessmentTitle { get; }
    public string SelfAssessmentHint { get; }
    public string StartPhq9Label { get; }
    public string StartGad7Label { get; }
    public string SubmitAssessmentLabel { get; }
    public string CancelAssessmentLabel { get; }
    public string SelectAnswerLabel { get; }

    public ObservableCollection<MentalHealthAssessmentQuestionItem> AssessmentQuestions { get; } = [];

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

    public bool ShowAssessmentForm
    {
        get => _showAssessmentForm;
        set => SetProperty(ref _showAssessmentForm, value);
    }

    public string AssessmentTitle
    {
        get => _assessmentTitle;
        set => SetProperty(ref _assessmentTitle, value);
    }

    public string AssessmentInstructions
    {
        get => _assessmentInstructions;
        set => SetProperty(ref _assessmentInstructions, value);
    }

    public bool SubmittingAssessment
    {
        get => _submittingAssessment;
        set
        {
            if (SetProperty(ref _submittingAssessment, value))
                ((Command)SubmitAssessmentCommand).ChangeCanExecute();
        }
    }

    public string? AssessmentMessage
    {
        get => _assessmentMessage;
        set => SetProperty(ref _assessmentMessage, value);
    }

    public bool CanSubmitAssessment =>
        !SubmittingAssessment
        && ShowAssessmentForm
        && AssessmentQuestions.Count > 0
        && AssessmentQuestions.All(q => q.SelectedScore is >= 0 and <= 3);

    public ICommand SelectMoodCommand { get; }
    public ICommand BookTherapyCommand { get; }
    public ICommand SessionHistoryCommand { get; }
    public ICommand StartPhq9Command { get; }
    public ICommand StartGad7Command { get; }
    public ICommand CancelAssessmentCommand { get; }
    public ICommand SubmitAssessmentCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var contentTask = _mentalHealth.GetContentAsync(CancellationToken.None);
            var moodsTask = _mentalHealth.GetMyMoodCheckInsAsync(14, CancellationToken.None);
            await Task.WhenAll(contentTask, moodsTask).ConfigureAwait(false);

            var content = await contentTask.ConfigureAwait(false);
            var moods = await moodsTask.ConfigureAwait(false);

            if (!content.IsSuccess || content.Data is null)
            {
                ErrorMessage = content.ErrorMessage ?? T("MentalHealthLoadFailed");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MedicalDisclaimer = content.Data.MedicalDisclaimer;
                ApplyMoodInsight(moods.IsSuccess ? moods.Data : null);
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyMoodInsight(IReadOnlyList<PatientMoodCheckInViewModel>? moods)
    {
        var kind = HomeWellnessInsightRules.Classify(moods?.Select(m => m.MoodScore) ?? Enumerable.Empty<int>());
        if (kind == HomeWellnessInsightRules.Kind.None)
        {
            InsightTitle = T("HomeDailyHealthTip");
            InsightBody = T("HomeWellnessNoMood");
            return;
        }

        InsightTitle = T("HomeWellnessTitleFromMood");
        InsightBody = kind switch
        {
            HomeWellnessInsightRules.Kind.Positive => T("HomeWellnessPositive"),
            HomeWellnessInsightRules.Kind.Steady => T("HomeWellnessSteady"),
            HomeWellnessInsightRules.Kind.Mixed => T("HomeWellnessMixed"),
            HomeWellnessInsightRules.Kind.Low => T("HomeWellnessLow"),
            _ => T("HomeWellnessNoMood"),
        };
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
            return;
        }

        var moods = await _mentalHealth.GetMyMoodCheckInsAsync(14, CancellationToken.None).ConfigureAwait(false);
        await MainThread.InvokeOnMainThreadAsync(() =>
            ApplyMoodInsight(moods.IsSuccess ? moods.Data : null));
    }

    private async Task BeginAssessmentAsync(string assessmentType)
    {
        AssessmentMessage = null;
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _mentalHealth.GetInstrumentAsync(assessmentType, CancellationToken.None)
                .ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                AssessmentMessage = response.ErrorMessage ?? T("MentalHealthInstrumentLoadFailed");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _activeAssessmentType = response.Data.AssessmentType;
                AssessmentTitle = response.Data.Title;
                AssessmentInstructions = response.Data.Instructions;
                AssessmentQuestions.Clear();
                var optionLabels = response.Data.Options
                    .Select(o => $"{o.Label} ({o.NumericScore})")
                    .ToList();
                foreach (var q in response.Data.Questions)
                {
                    var item = new MentalHealthAssessmentQuestionItem(
                        q.Order,
                        q.QuestionText,
                        optionLabels,
                        OnQuestionAnswerChanged);
                    AssessmentQuestions.Add(item);
                }

                ShowAssessmentForm = true;
                ((Command)SubmitAssessmentCommand).ChangeCanExecute();
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnQuestionAnswerChanged() =>
        ((Command)SubmitAssessmentCommand).ChangeCanExecute();

    private void CancelAssessment()
    {
        ShowAssessmentForm = false;
        AssessmentQuestions.Clear();
        _activeAssessmentType = string.Empty;
        AssessmentTitle = string.Empty;
        AssessmentInstructions = string.Empty;
        AssessmentMessage = null;
        ((Command)SubmitAssessmentCommand).ChangeCanExecute();
    }

    private async Task SubmitAssessmentAsync()
    {
        if (!CanSubmitAssessment)
            return;

        var clinicId = _clinicIdProvider.GetClinicId();
        if (clinicId is null || clinicId == Guid.Empty)
        {
            AssessmentMessage = T("MentalHealthClinicRequired");
            return;
        }

        SubmittingAssessment = true;
        AssessmentMessage = null;
        try
        {
            var request = new SubmitPatientMentalHealthAssessmentRequest
            {
                ClinicId = clinicId.Value,
                AssessmentType = _activeAssessmentType,
                Answers = AssessmentQuestions
                    .Select(q => new CreateMentalHealthAssessmentAnswerRequest
                    {
                        Order = q.Order,
                        NumericScore = q.SelectedScore!.Value
                    })
                    .ToList()
            };

            var response = await _mentalHealth.SubmitAssessmentAsync(request, CancellationToken.None)
                .ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                AssessmentMessage = response.ErrorMessage ?? T("MentalHealthSubmitFailed");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CancelAssessment();
                AssessmentMessage = Format(
                    T("MentalHealthSubmitSuccessFormat"),
                    response.Data.AssessmentType,
                    response.Data.TotalScore?.ToString("0", CultureInfo.CurrentCulture) ?? "0",
                    response.Data.SeverityLevel);
            });
        }
        finally
        {
            SubmittingAssessment = false;
        }
    }
}

public sealed class MentalHealthAssessmentQuestionItem : System.ComponentModel.INotifyPropertyChanged
{
    private int _selectedOptionIndex = -1;
    private readonly Action _onChanged;

    public MentalHealthAssessmentQuestionItem(
        int order,
        string questionText,
        IReadOnlyList<string> optionLabels,
        Action onChanged)
    {
        Order = order;
        QuestionText = questionText;
        OptionLabels = optionLabels;
        _onChanged = onChanged;
    }

    public int Order { get; }
    public string QuestionText { get; }
    public IReadOnlyList<string> OptionLabels { get; }

    public int SelectedOptionIndex
    {
        get => _selectedOptionIndex;
        set
        {
            if (_selectedOptionIndex == value)
                return;
            _selectedOptionIndex = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(SelectedOptionIndex)));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(SelectedScore)));
            _onChanged();
        }
    }

    public int? SelectedScore => SelectedOptionIndex is >= 0 and <= 3 ? SelectedOptionIndex : null;

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}
