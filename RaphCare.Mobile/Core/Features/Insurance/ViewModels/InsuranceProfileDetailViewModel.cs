using System.Globalization;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Insurance;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Insurance.ViewModels;

public sealed class InsuranceProfileDetailViewModel : BaseViewModel
{
    private readonly IPatientInsuranceService _insurance;
    private Guid _profileId;
    private string _planLine = string.Empty;
    private string _membership = string.Empty;
    private string _startLine = string.Empty;
    private string _endLine = string.Empty;
    private bool _isActive;
    private string? _errorMessage;

    public InsuranceProfileDetailViewModel(IPatientInsuranceService insurance)
    {
        _insurance = insurance ?? throw new ArgumentNullException(nameof(insurance));
        Title = AppResources.T("InsuranceDetailTitle");
        PlanLabel = AppResources.T("InsuranceDetailPlan");
        MembershipLabel = AppResources.T("InsuranceMembershipLabel");
        StartLabel = AppResources.T("InsuranceStartDateLabel");
        EndLabel = AppResources.T("InsuranceEndDateLabel");
        ActiveLabel = AppResources.T("InsuranceActiveLabel");
        SaveLabel = AppResources.T("InsuranceSave");
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public string PlanLabel { get; }
    public string MembershipLabel { get; }
    public string StartLabel { get; }
    public string EndLabel { get; }
    public string ActiveLabel { get; }
    public string SaveLabel { get; }

    public string PlanLine
    {
        get => _planLine;
        set => SetProperty(ref _planLine, value);
    }

    public string MembershipText
    {
        get => _membership;
        set => SetProperty(ref _membership, value);
    }

    public string StartLine
    {
        get => _startLine;
        set => SetProperty(ref _startLine, value);
    }

    public string EndLine
    {
        get => _endLine;
        set => SetProperty(ref _endLine, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackCommand { get; }
    public ICommand SaveCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("profileId", out var v) && v != null && Guid.TryParse(v.ToString(), out var id))
            _profileId = id;
    }

    public async Task LoadAsync()
    {
        if (_profileId == Guid.Empty)
        {
            ErrorMessage = AppResources.T("InsuranceDetailFailed");
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _insurance.GetMyProfileAsync(_profileId, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? AppResources.T("InsuranceDetailFailed");
                return;
            }

            ApplyProfile(response.Data);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyProfile(PatientInsuranceProfileViewModel p)
    {
        var culture = CultureInfo.CurrentCulture;
        var name = string.IsNullOrWhiteSpace(p.PlanName) ? p.PlanCode : p.PlanName;
        if (!string.IsNullOrWhiteSpace(p.PlanCode) && !string.IsNullOrWhiteSpace(p.PlanName))
            name = $"{p.PlanName} ({p.PlanCode})";
        PlanLine = string.IsNullOrWhiteSpace(name) ? "—" : name.Trim();
        MembershipText = p.MembershipNumber;
        StartLine = p.StartDate.ToLocalTime().ToString("d", culture);
        EndLine = p.EndDate.HasValue ? p.EndDate.Value.ToLocalTime().ToString("d", culture) : "—";
        IsActive = p.IsActive;
    }

    private async Task SaveAsync()
    {
        if (_profileId == Guid.Empty) return;

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var req = new UpdatePatientInsuranceProfileRequest
            {
                Id = _profileId,
                EndDate = null,
                IsActive = IsActive
            };

            var response = await _insurance.UpdateProfileAsync(req, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess)
                ErrorMessage = response.ErrorMessage ?? AppResources.T("InsuranceSaveFailed");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
