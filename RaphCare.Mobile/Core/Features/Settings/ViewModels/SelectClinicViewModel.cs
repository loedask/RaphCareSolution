using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Clinics;
using RaphCare.Mobile.Core.Common.Services.Api;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Core.Features.Auth.Models;
using RaphCare.Mobile.Core.Features.Settings.Models;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>
/// Sets the active clinic on this phone (API tenant header). Directory search is not hospital membership.
/// Linked hospitals come from the patient's care links on the server.
/// </summary>
public sealed class SelectClinicViewModel : BaseViewModel
{
    private readonly IEmailAuthService _emailAuth;
    private readonly IPatientClinicsService _patientClinics;
    private readonly ISelectedClinicStore _selectedClinic;

    private string _searchText = string.Empty;
    private string _referenceCodeText = string.Empty;
    private string? _errorMessage;
    private string? _statusMessage;
    private string _currentClinicSummary = string.Empty;
    private bool _hasSearchResults;
    private bool _showResultsCard;
    private bool _hasLinkedClinics;
    private bool _showLinkedEmpty;
    private bool _linkedLoadFailed;
    private string _linkedLoadFailedText;

    public SelectClinicViewModel(
        IEmailAuthService emailAuth,
        IPatientClinicsService patientClinics,
        ISelectedClinicStore selectedClinic)
    {
        _emailAuth = emailAuth ?? throw new ArgumentNullException(nameof(emailAuth));
        _patientClinics = patientClinics ?? throw new ArgumentNullException(nameof(patientClinics));
        _selectedClinic = selectedClinic ?? throw new ArgumentNullException(nameof(selectedClinic));

        Title = T("SelectClinicTitle");
        HintText = T("SelectClinicHint");
        ActiveClinicLabel = T("SelectClinicActiveLabel");
        LinkedHeading = T("SelectClinicLinkedHeading");
        LinkedEmptyText = T("SelectClinicLinkedEmpty");
        LinkedTapHint = T("SelectClinicLinkedTapHint");
        LinkedActiveBadge = T("SelectClinicLinkedActiveBadge");
        _linkedLoadFailedText = T("SelectClinicLinkedLoadFailed");
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
        LinkedClinics = new ObservableCollection<LinkedClinicDisplayItem>();
        SearchCommand = new Command(async () => await SearchAsync(), () => !IsBusy);
        UseCodeCommand = new Command(async () => await UseCodeAsync(), () => !IsBusy);
        SelectCommand = new Command<ClinicPickerItem>(async item => await SelectAsync(item), _ => !IsBusy);
        SelectLinkedCommand = new Command<LinkedClinicDisplayItem>(async item => await SelectLinkedAsync(item), _ => !IsBusy);
        ClearCommand = new Command(ClearSelection, () => !IsBusy);
        RefreshCurrentSummary();
    }

    public string HintText { get; }
    public string ActiveClinicLabel { get; }
    public string LinkedHeading { get; }
    public string LinkedEmptyText { get; }
    public string LinkedTapHint { get; }
    public string LinkedActiveBadge { get; }

    public string LinkedLoadFailedText
    {
        get => _linkedLoadFailedText;
        private set => SetProperty(ref _linkedLoadFailedText, value);
    }

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
    public ObservableCollection<LinkedClinicDisplayItem> LinkedClinics { get; }

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

    public bool HasLinkedClinics
    {
        get => _hasLinkedClinics;
        private set => SetProperty(ref _hasLinkedClinics, value);
    }

    public bool ShowLinkedEmpty
    {
        get => _showLinkedEmpty;
        private set => SetProperty(ref _showLinkedEmpty, value);
    }

    public bool LinkedLoadFailed
    {
        get => _linkedLoadFailed;
        private set => SetProperty(ref _linkedLoadFailed, value);
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
    public ICommand SelectLinkedCommand { get; }
    public ICommand ClearCommand { get; }

    public async Task LoadAsync()
    {
        RefreshCurrentSummary();
        ClearResultsUi(showCard: false);
        StatusMessage = null;
        ErrorMessage = null;
        if (_selectedClinic.ClinicId is not null
            && !string.IsNullOrWhiteSpace(_selectedClinic.ReferenceCode)
            && string.IsNullOrWhiteSpace(ReferenceCodeText))
        {
            ReferenceCodeText = _selectedClinic.ReferenceCode;
        }

        await LoadLinkedClinicsAsync().ConfigureAwait(false);
    }

    private async Task LoadLinkedClinicsAsync()
    {
        var fallback = T("SelectClinicLinkedLoadFailed");
        try
        {
            var response = await _patientClinics.GetMyLinkedClinicsAsync(CancellationToken.None).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LinkedClinics.Clear();
                if (!response.IsSuccess || response.Data is null)
                {
                    LinkedLoadFailedText = LinkedClinicLoadFailureMessage.Resolve(response.ErrorMessage, fallback);
                    LinkedLoadFailed = true;
                    HasLinkedClinics = false;
                    ShowLinkedEmpty = false;
                    return;
                }

                LinkedLoadFailed = false;
                LinkedLoadFailedText = fallback;
                var activeId = _selectedClinic.ClinicId;
                foreach (var clinic in response.Data)
                {
                    LinkedClinics.Add(new LinkedClinicDisplayItem
                    {
                        ClinicId = clinic.ClinicId,
                        Name = clinic.Name,
                        ReferenceCode = clinic.ReferenceCode ?? string.Empty,
                        AccessLabel = T(LinkedClinicAccessLabelRules.ResourceKey(clinic.AccessKind)),
                        IsActiveOnPhone = activeId is { } id && id == clinic.ClinicId,
                    });
                }

                HasLinkedClinics = LinkedClinics.Count > 0;
                ShowLinkedEmpty = LinkedClinics.Count == 0;
            });
        }
        catch
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LinkedClinics.Clear();
                LinkedLoadFailedText = fallback;
                LinkedLoadFailed = true;
                HasLinkedClinics = false;
                ShowLinkedEmpty = false;
            });
        }
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
            RaiseCanExecuteChanged(SearchCommand, UseCodeCommand, ClearCommand, SelectLinkedCommand);
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
            RaiseCanExecuteChanged(SearchCommand, UseCodeCommand, ClearCommand, SelectLinkedCommand);
        }
    }

    private async Task SelectAsync(ClinicPickerItem? item)
    {
        if (item?.Id is not { } id)
            return;

        await ApplyClinicAsync(id, item.Name, item.ReferenceCode).ConfigureAwait(false);
    }

    private async Task SelectLinkedAsync(LinkedClinicDisplayItem? item)
    {
        if (item is null)
            return;

        await ApplyClinicAsync(item.ClinicId, item.Name, item.ReferenceCode).ConfigureAwait(false);
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
            RefreshLinkedActiveFlags();
        });
    }

    private void RefreshLinkedActiveFlags()
    {
        var activeId = _selectedClinic.ClinicId;
        var snapshot = LinkedClinics.ToList();
        LinkedClinics.Clear();
        foreach (var clinic in snapshot)
        {
            LinkedClinics.Add(new LinkedClinicDisplayItem
            {
                ClinicId = clinic.ClinicId,
                Name = clinic.Name,
                ReferenceCode = clinic.ReferenceCode,
                AccessLabel = clinic.AccessLabel,
                IsActiveOnPhone = activeId is { } id && id == clinic.ClinicId,
            });
        }
    }

    private void ClearSelection()
    {
        _selectedClinic.Clear();
        RefreshCurrentSummary();
        StatusMessage = T("SelectClinicCleared");
        ErrorMessage = null;
        RefreshLinkedActiveFlags();
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
