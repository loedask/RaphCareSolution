using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Clinics;
using RaphCare.Mobile.Core.Common.Services.Api;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Core.Features.Auth.Models;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>
/// Sets the active clinic on this phone (API tenant header). Directory search is not hospital membership.
/// </summary>
public sealed class SelectClinicViewModel : BaseViewModel
{
    private readonly IEmailAuthService _emailAuth;
    private readonly ISelectedClinicStore _selectedClinic;

    private string _searchText = string.Empty;
    private string _referenceCodeText = string.Empty;
    private string? _errorMessage;
    private string? _statusMessage;
    private string _currentClinicSummary = string.Empty;
    private bool _hasSearchResults;
    private bool _showResultsCard;

    public SelectClinicViewModel(IEmailAuthService emailAuth, ISelectedClinicStore selectedClinic)
    {
        _emailAuth = emailAuth ?? throw new ArgumentNullException(nameof(emailAuth));
        _selectedClinic = selectedClinic ?? throw new ArgumentNullException(nameof(selectedClinic));

        Title = T("SelectClinicTitle");
        HintText = T("SelectClinicHint");
        ActiveClinicLabel = T("SelectClinicActiveLabel");
        SearchLabel = T("SelectClinicSearchLabel");
        SearchPlaceholder = T("SelectClinicSearchPlaceholder");
        ReferenceLabel = T("SelectClinicReferenceLabel");
        ReferencePlaceholder = T("SelectClinicReferencePlaceholder");
        SearchButtonText = T("SelectClinicSearchButton");
        UseCodeButtonText = T("SelectClinicUseCodeButton");
        ClearButtonText = T("SelectClinicClearButton");
        ResultsHeading = T("SelectClinicResultsHeading");
        ResultsEmptyHint = T("SelectClinicResultsEmptyHint");
        TapToActivateHint = T("SelectClinicTapToActivate");

        Results = new ObservableCollection<ClinicPickerItem>();
        SearchCommand = new Command(async () => await SearchAsync(), () => !IsBusy);
        UseCodeCommand = new Command(async () => await UseCodeAsync(), () => !IsBusy);
        SelectCommand = new Command<ClinicPickerItem>(async item => await SelectAsync(item), _ => !IsBusy);
        ClearCommand = new Command(ClearSelection, () => !IsBusy);
        RefreshCurrentSummary();
    }

    public string HintText { get; }
    public string ActiveClinicLabel { get; }
    public string SearchLabel { get; }
    public string SearchPlaceholder { get; }
    public string ReferenceLabel { get; }
    public string ReferencePlaceholder { get; }
    public string SearchButtonText { get; }
    public string UseCodeButtonText { get; }
    public string ClearButtonText { get; }
    public string ResultsHeading { get; }
    public string ResultsEmptyHint { get; }
    public string TapToActivateHint { get; }

    public ObservableCollection<ClinicPickerItem> Results { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value ?? string.Empty);
    }

    public string ReferenceCodeText
    {
        get => _referenceCodeText;
        set => SetProperty(ref _referenceCodeText, value ?? string.Empty);
    }

    public string CurrentClinicSummary
    {
        get => _currentClinicSummary;
        private set => SetProperty(ref _currentClinicSummary, value);
    }

    public bool HasSearchResults
    {
        get => _hasSearchResults;
        private set => SetProperty(ref _hasSearchResults, value);
    }

    public bool ShowResultsCard
    {
        get => _showResultsCard;
        private set => SetProperty(ref _showResultsCard, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand UseCodeCommand { get; }
    public ICommand SelectCommand { get; }
    public ICommand ClearCommand { get; }

    public Task LoadAsync()
    {
        RefreshCurrentSummary();
        // Do not auto-load the clinic directory. That list looks like membership.
        ClearResultsUi(showCard: false);
        StatusMessage = null;
        ErrorMessage = null;
        if (_selectedClinic.ClinicId is not null
            && !string.IsNullOrWhiteSpace(_selectedClinic.ReferenceCode)
            && string.IsNullOrWhiteSpace(ReferenceCodeText))
        {
            ReferenceCodeText = _selectedClinic.ReferenceCode;
        }

        return Task.CompletedTask;
    }

    private void RefreshCurrentSummary()
    {
        if (_selectedClinic.ClinicId is null)
        {
            CurrentClinicSummary = T("SelectClinicNoneSelected");
            return;
        }

        var name = _selectedClinic.ClinicName ?? T("SelectClinicUnknownName");
        var code = _selectedClinic.ReferenceCode;
        CurrentClinicSummary = string.IsNullOrWhiteSpace(code)
            ? Format(T("SelectClinicCurrentFormat"), name)
            : Format(T("SelectClinicCurrentWithCodeFormat"), name, code);
    }

    private async Task SearchAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;

        if (!SelectClinicSearchRules.TryNormalizeQuery(SearchText, out var query))
        {
            ClearResultsUi(showCard: false);
            StatusMessage = T("SelectClinicSearchRequired");
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _emailAuth
                .GetRegistrationClinicsAsync(query)
                .ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Results.Clear();
                if (!response.IsSuccess || response.Data is null)
                {
                    ErrorMessage = response.ErrorMessage ?? T("SelectClinicLoadFailed");
                    HasSearchResults = false;
                    ShowResultsCard = false;
                    return;
                }

                foreach (var clinic in response.Data)
                    Results.Add(ToPickerItem(clinic));

                HasSearchResults = Results.Count > 0;
                ShowResultsCard = true;
                if (Results.Count == 0)
                    StatusMessage = T("SelectClinicNoResults");
            });
        }
        catch (Exception)
        {
            ErrorMessage = T("SelectClinicLoadFailed");
            ClearResultsUi(showCard: false);
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged(SearchCommand, UseCodeCommand, ClearCommand);
        }
    }

    private async Task UseCodeAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;
        if (string.IsNullOrWhiteSpace(ReferenceCodeText))
        {
            ErrorMessage = T("SelectClinicReferenceRequired");
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _emailAuth
                .ResolveClinicByReferenceAsync(ReferenceCodeText.Trim())
                .ConfigureAwait(false);

            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("SelectClinicReferenceNotFound");
                return;
            }

            await ApplyClinicAsync(response.Data.Id, response.Data.Name, response.Data.ReferenceCode)
                .ConfigureAwait(false);
            ClearResultsUi(showCard: false);
        }
        catch (Exception)
        {
            ErrorMessage = T("SelectClinicReferenceNotFound");
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged(SearchCommand, UseCodeCommand, ClearCommand);
        }
    }

    private async Task SelectAsync(ClinicPickerItem? item)
    {
        if (item?.Id is not { } id)
            return;

        await ApplyClinicAsync(id, item.Name, item.ReferenceCode).ConfigureAwait(false);
    }

    private async Task ApplyClinicAsync(Guid id, string name, string referenceCode)
    {
        _selectedClinic.SetClinic(id, name, referenceCode);
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            RefreshCurrentSummary();
            if (!string.IsNullOrWhiteSpace(referenceCode))
                ReferenceCodeText = referenceCode;
            StatusMessage = T("SelectClinicSaved");
            ErrorMessage = null;
        });
    }

    private void ClearSelection()
    {
        _selectedClinic.Clear();
        RefreshCurrentSummary();
        StatusMessage = T("SelectClinicCleared");
        ErrorMessage = null;
    }

    private void ClearResultsUi(bool showCard)
    {
        Results.Clear();
        HasSearchResults = false;
        ShowResultsCard = showCard;
    }

    private static ClinicPickerItem ToPickerItem(RegistrationClinicItem clinic)
    {
        var code = clinic.ReferenceCode?.Trim() ?? string.Empty;
        var display = string.IsNullOrEmpty(code)
            ? clinic.Name
            : $"{clinic.Name} ({code})";
        return new ClinicPickerItem
        {
            Id = clinic.Id,
            Name = clinic.Name,
            ReferenceCode = code,
            DisplayName = display
        };
    }
}
