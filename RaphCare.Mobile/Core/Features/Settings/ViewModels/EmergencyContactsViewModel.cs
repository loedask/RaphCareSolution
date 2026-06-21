using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Settings.Models;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Emergency contacts (concept <c>EmergencyContacts.tsx</c>).</summary>
public sealed class EmergencyContactsViewModel : BaseViewModel
{
    private readonly ILocalPatientProfileStore _local;
    private bool _showForm;
    private Guid? _editId;
    private string _formName = string.Empty;
    private string _formRelationship = string.Empty;
    private string _formPhone = string.Empty;

    public EmergencyContactsViewModel(ILocalPatientProfileStore local)
    {
        _local = local;
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
        CancelFormButtonText = T("CommonCancel");

        Contacts = new ObservableCollection<StoredEmergencyContact>();
        AddCommand = new Command(() => ShowFormFor(null));
        CancelFormCommand = new Command(ResetForm);
        SaveFormCommand = new Command(SaveForm);
        EditCommand = new Command<StoredEmergencyContact>(c => ShowFormFor(c));
        RemoveCommand = new Command<StoredEmergencyContact>(RemoveContact);
    }

    public ObservableCollection<StoredEmergencyContact> Contacts { get; }

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

    public bool ShowEmpty => !IsBusy && Contacts.Count == 0;

    public ICommand AddCommand { get; }
    public ICommand CancelFormCommand { get; }
    public ICommand SaveFormCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand RemoveCommand { get; }

    public void LoadFromStore()
    {
        Contacts.Clear();
        foreach (var c in _local.GetEmergencyContacts())
            Contacts.Add(c);
        OnPropertyChanged(nameof(ShowEmpty));
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

    private void SaveForm()
    {
        if (string.IsNullOrWhiteSpace(FormName) || string.IsNullOrWhiteSpace(FormRelationship) || string.IsNullOrWhiteSpace(FormPhone))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await Shell.Current.DisplayAlertAsync(Title, T("EmergencyContactsMissingFields"), T("CommonOk")));
            return;
        }

        if (_editId is { } id)
        {
            var existing = Contacts.FirstOrDefault(c => c.Id == id);
            if (existing is not null)
            {
                existing.Name = FormName.Trim();
                existing.Relationship = FormRelationship.Trim();
                existing.Phone = FormPhone.Trim();
            }
        }
        else
        {
            Contacts.Add(new StoredEmergencyContact
            {
                Name = FormName.Trim(),
                Relationship = FormRelationship.Trim(),
                Phone = FormPhone.Trim(),
            });
        }

        Persist();
        ResetForm();
        OnPropertyChanged(nameof(ShowEmpty));
    }

    private void RemoveContact(StoredEmergencyContact? contact)
    {
        if (contact is null) return;
        Contacts.Remove(contact);
        Persist();
        OnPropertyChanged(nameof(ShowEmpty));
        MainThread.BeginInvokeOnMainThread(async () =>
            await Shell.Current.DisplayAlertAsync(Title, RemovedText, T("CommonOk")));
    }

    private void Persist() => _local.SetEmergencyContacts(Contacts.ToList());
}
