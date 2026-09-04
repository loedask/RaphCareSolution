using System.Collections.ObjectModel;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Insurance;
using RaphCare.Mobile.Core.Features.Insurance.Models;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Insurance.ViewModels;

public sealed class AddInsuranceProfileViewModel : BaseViewModel
{
    private readonly IPatientInsuranceService _insurance;
    private string _membershipNumber = string.Empty;
    private DateTime _startDate = DateTime.Today;
    private InsurancePlanPickerItem? _selectedPlan;
    private string? _errorMessage;

    public AddInsuranceProfileViewModel(IPatientInsuranceService insurance)
    {
        _insurance = insurance ?? throw new ArgumentNullException(nameof(insurance));
        Title = T("InsuranceAddTitle");
        MembershipLabel = T("InsuranceMembershipLabel");
        PlanLabel = T("InsurancePlanLabel");
        StartDateLabel = T("InsuranceStartDateLabel");
        SubmitLabel = T("InsuranceSubmit");
        CancelLabel = T("InsuranceCancel");

        Plans = new ObservableCollection<InsurancePlanPickerItem>();

        SubmitCommand = new Command(async () => await SubmitAsync());
        CancelCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
    }

    public ObservableCollection<InsurancePlanPickerItem> Plans { get; }

    public string MembershipLabel { get; }
    public string PlanLabel { get; }
    public string StartDateLabel { get; }
    public string SubmitLabel { get; }
    public string CancelLabel { get; }

    public string MembershipNumber
    {
        get => _membershipNumber;
        set => SetProperty(ref _membershipNumber, value ?? string.Empty);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public InsurancePlanPickerItem? SelectedPlan
    {
        get => _selectedPlan;
        set => SetProperty(ref _selectedPlan, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }

    public async Task LoadPlansAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _insurance.GetActivePlansAsync(CancellationToken.None).ConfigureAwait(false);
            await RunOnMainThreadAsync(() =>
            {
                SelectedPlan = null;
                Plans.Clear();

                if (!response.IsSuccess || response.Data is null)
                {
                    ErrorMessage = response.ErrorMessage ?? T("InsurancePlansLoadFailed");
                    return;
                }

                foreach (var p in response.Data)
                {
                    Plans.Add(new InsurancePlanPickerItem
                    {
                        Id = p.Id,
                        DisplayName = string.IsNullOrWhiteSpace(p.Code) ? p.Name : $"{p.Name} ({p.Code})"
                    });
                }

                SelectedPlan = Plans.Count > 0 ? Plans[0] : null;
            }).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await RunOnMainThreadAsync(() =>
            {
                ErrorMessage = T("InsurancePlansLoadFailed");
                SelectedPlan = null;
                Plans.Clear();
            }).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SubmitAsync()
    {
        if (SelectedPlan is null)
        {
            ErrorMessage = T("InsuranceSelectPlan");
            return;
        }

        if (string.IsNullOrWhiteSpace(MembershipNumber))
        {
            ErrorMessage = T("InsuranceMembershipRequired");
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var utcStart = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 0, 0, 0, DateTimeKind.Utc);
            var req = new CreatePatientInsuranceProfileRequest
            {
                InsurancePlanId = SelectedPlan.Id,
                MembershipNumber = MembershipNumber.Trim(),
                StartDate = utcStart
            };

            var response = await _insurance.CreateProfileAsync(req, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess)
            {
                ErrorMessage = response.ErrorMessage ?? T("InsuranceCreateFailed");
                return;
            }

            await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
