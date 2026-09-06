using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Settings;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Core.Features.Settings.Models;
using RaphCare.Mobile.Core.Features.Settings.Services;
using MauiContacts = Microsoft.Maui.ApplicationModel.Communication.Contacts;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Emergency contacts (concept <c>EmergencyContacts.tsx</c>). Synced via <c>api/patient/emergency-contacts</c>.</summary>
public sealed class EmergencyContactsViewModel : BaseViewModel
{
    private readonly IPatientEmergencyContactsService _api;
    private readonly ILocalPatientProfileStore _local;
    private bool _showForm;
    private Guid? _editId;
    private string _formName = string.Empty;
    private string _formRelationship = string.Empty;
    private string _formPhone = string.Empty;
    private string? _errorMessage;

    public EmergencyContactsViewModel(IPatientEmergencyContactsService api, ILocalPatientProfileStore local)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _local = local ?? throw new ArgumentNullException(nameof(local));
        Title = T("EmergencyContactsTitle");
        AddButtonText = T("EmergencyContactsAdd");
        SaveContactText = T("EmergencyContactsSave");
        UpdateContactText = T("EmergencyContactsUpdate");
        FormTitleAdd = T("EmergencyContactsNew");
        FormTitleEdit = T("EmergencyContactsEdit");
        NameLabel = T("EmergencyContactsName");
        RelationshipLabel = T("EmergencyContactsRelationship");
        PhoneLabel = T("EmergencyContactsPhone");
        EmptyText = T("EmergencyContactsEmpty");
        RemovedText = T("EmergencyContactsRemoved");
        CancelFormButtonText = T("CommonCancel");

        Contacts = new ObservableCollection<StoredEmergencyContact>();
        Contacts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowEmpty));
        AddCommand = new Command(async () => await BeginAddAsync());
        CancelFormCommand = new Command(ResetForm);
        SaveFormCommand = new Command(async () => await SaveFormAsync());
        EditCommand = new Command<StoredEmergencyContact>(c => ShowFormFor(c));
        RemoveCommand = new Command<StoredEmergencyContact>(async c => await RemoveContactAsync(c));
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public ObservableCollection<StoredEmergencyContact> Contacts { get; }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowForm
    {
        get => _showForm;
        set => SetProperty(ref _showForm, value);
    }

    public bool IsEditing => _editId.HasValue;

    public string FormTitle => IsEditing ? FormTitleEdit : FormTitleAdd;
    public string SaveFormButtonText => IsEditing ? UpdateContactText : SaveContactText;

    public string FormName
    {
        get => _formName;
        set => SetProperty(ref _formName, value);
    }

    public string FormRelationship
    {
        get => _formRelationship;
        set => SetProperty(ref _formRelationship, value);
    }

    public string FormPhone
    {
        get => _formPhone;
        set => SetProperty(ref _formPhone, value);
    }

    public string AddButtonText { get; }
    public string SaveContactText { get; }
    public string UpdateContactText { get; }
    public string FormTitleAdd { get; }
    public string FormTitleEdit { get; }
    public string NameLabel { get; }
    public string RelationshipLabel { get; }
    public string PhoneLabel { get; }
    public string EmptyText { get; }
    public string RemovedText { get; }
    public string CancelFormButtonText { get; }

    public bool ShowEmpty => !IsBusy && Contacts.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public ICommand AddCommand { get; }
    public ICommand CancelFormCommand { get; }
    public ICommand SaveFormCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        OnPropertyChanged(nameof(ShowEmpty));
        try
        {
            var response = await _api.GetMyEmergencyContactsAsync(CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                var failMessage = response.ErrorMessage ?? T("EmergencyContactsLoadFailed");
                await RunOnMainThreadAsync(() =>
                {
                    ErrorMessage = failMessage;
                    LoadFromLocalCache();
                }).ConfigureAwait(false);
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Contacts.Clear();
                foreach (var c in response.Data)
                {
                    Contacts.Add(new StoredEmergencyContact
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Relationship = c.Relationship ?? string.Empty,
                        Phone = c.PhoneNumber ?? string.Empty,
                    });
                }

                SyncLocalCache();
                OnPropertyChanged(nameof(ShowEmpty));
            });
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(ShowEmpty));
        }
    }

    private void LoadFromLocalCache()
    {
        Contacts.Clear();
        foreach (var c in _local.GetEmergencyContacts())
            Contacts.Add(c);
        OnPropertyChanged(nameof(ShowEmpty));
    }

    private void SyncLocalCache() => _local.SetEmergencyContacts(Contacts.ToList());

    private async Task BeginAddAsync()
    {
        if (ShowForm || IsBusy)
            return;

        var fromPhone = T("EmergencyContactsFromPhone");
        var enterManually = T("EmergencyContactsEnterManually");
        var choice = await DisplayActionSheetSafeAsync(
            T("EmergencyContactsAddHowTitle"),
            T("CommonCancel"),
            destruction: null,
            fromPhone,
            enterManually).ConfigureAwait(false);

        if (choice is null)
            return;

        if (string.Equals(choice, fromPhone, StringComparison.Ordinal))
        {
            await ImportFromPhoneContactsAsync().ConfigureAwait(false);
            return;
        }

        if (string.Equals(choice, enterManually, StringComparison.Ordinal))
            ShowFormFor(null);
    }

    private async Task ImportFromPhoneContactsAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.ContactsRead>().ConfigureAwait(false);
        if (status != PermissionStatus.Granted)
        {
            await AlertAsync(T("EmergencyContactsPermissionDenied")).ConfigureAwait(false);
            return;
        }

        Contact? picked;
        try
        {
            picked = await MainThread.InvokeOnMainThreadAsync(
                () => MauiContacts.Default.PickContactAsync()).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await AlertAsync(T("EmergencyContactsPickFailed")).ConfigureAwait(false);
            return;
        }

        if (picked is null)
            return;

        var phoneChoices = EmergencyContactPhonePickerRules.DistinctPhoneChoices(
            picked.Phones?.Select(p => p.PhoneNumber));

        if (phoneChoices.Count == 0)
        {
            await AlertAsync(T("EmergencyContactsNoPhoneOnContact")).ConfigureAwait(false);
            ShowFormFor(null);
            return;
        }

        string? chosenPhone = phoneChoices[0];
        if (phoneChoices.Count > 1)
        {
            chosenPhone = await DisplayActionSheetSafeAsync(
                T("EmergencyContactsPickPhoneTitle"),
                T("CommonCancel"),
                destruction: null,
                phoneChoices.ToArray()).ConfigureAwait(false);
            if (chosenPhone is null)
                return;
        }

        var mapped = EmergencyContactPhonePickerRules.MapFromPhoneContact(picked.DisplayName, [chosenPhone]);
        ShowFormForImport(mapped.Name, mapped.Phone);
    }

    private void ShowFormForImport(string name, string phone)
    {
        _editId = null;
        FormName = name;
        FormRelationship = string.Empty;
        FormPhone = phone;
        ShowForm = true;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveFormButtonText));
    }

    private void ShowFormFor(StoredEmergencyContact? contact)
    {
        if (contact is null)
        {
            _editId = null;
            FormName = string.Empty;
            FormRelationship = string.Empty;
            FormPhone = string.Empty;
        }
        else
        {
            _editId = contact.Id;
            FormName = contact.Name;
            FormRelationship = contact.Relationship;
            FormPhone = contact.Phone;
        }

        ShowForm = true;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveFormButtonText));
    }

    private void ResetForm()
    {
        _editId = null;
        ShowForm = false;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveFormButtonText));
    }

    private async Task SaveFormAsync()
    {
        if (string.IsNullOrWhiteSpace(FormName) || string.IsNullOrWhiteSpace(FormRelationship) || string.IsNullOrWhiteSpace(FormPhone))
        {
            await AlertAsync(T("EmergencyContactsMissingFields"));
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        try
        {
            if (_editId is { } id)
            {
                var res = await _api.UpdateEmergencyContactAsync(
                    id,
                    FormName.Trim(),
                    FormRelationship.Trim(),
                    FormPhone.Trim(),
                    email: null,
                    CancellationToken.None).ConfigureAwait(false);
                if (!res.IsSuccess)
                {
                    await AlertAsync(res.ErrorMessage ?? T("EmergencyContactsSaveFailed"));
                    return;
                }
            }
            else
            {
                var res = await _api.AddEmergencyContactAsync(
                    FormName.Trim(),
                    FormRelationship.Trim(),
                    FormPhone.Trim(),
                    email: null,
                    CancellationToken.None).ConfigureAwait(false);
                if (!res.IsSuccess)
                {
                    await AlertAsync(res.ErrorMessage ?? T("EmergencyContactsSaveFailed"));
                    return;
                }
            }

            ResetForm();
            await LoadAsync().ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveContactAsync(StoredEmergencyContact? contact)
    {
        if (contact is null || IsBusy) return;
        IsBusy = true;
        try
        {
            var res = await _api.RemoveEmergencyContactAsync(contact.Id, CancellationToken.None).ConfigureAwait(false);
            if (!res.IsSuccess)
            {
                await AlertAsync(res.ErrorMessage ?? T("EmergencyContactsSaveFailed"));
                return;
            }

            await DisplayAlertSafeAsync(Title, RemovedText, T("CommonOk"));
            await LoadAsync().ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static Task AlertAsync(string message) =>
        DisplayAlertSafeAsync(T("EmergencyContactsTitle"), message, T("CommonOk"));
}
