using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Family.ViewModels;

public sealed class FamilyMemberDetailViewModel : BaseViewModel
{
    private readonly IPatientFamilyMembersService _family;
    private Guid _memberId;
    private Guid? _linkedPatientId;
    private string? _errorMessage;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string? _selectedRelationship;
    private bool _hasDateOfBirth;
    private DateTime _dobDate = DateTime.Today.AddYears(-10);
    private string _phone = string.Empty;
    private string _email = string.Empty;

    public FamilyMemberDetailViewModel(IPatientFamilyMembersService family)
    {
        _family = family ?? throw new ArgumentNullException(nameof(family));
        Title = T("FamilyDetailTitle");
        RelationshipOptions = ["Child", "Spouse", "Parent", "Sibling", "Other"];

        FirstNameLabel = T("FamilyFieldFirstName");
        LastNameLabel = T("FamilyFieldLastName");
        RelationshipLabel = T("FamilyFieldRelationship");
        DobLabel = T("FamilyFieldDob");
        PhoneLabel = T("FamilyFieldPhone");
        EmailLabel = T("FamilyFieldEmail");
        SaveLabel = T("FamilySave");
        RemoveLabel = T("FamilyRemove");

        SaveCommand = new Command(async () => await SaveAsync(), () => CanSave);
        RemoveCommand = new Command(async () => await RemoveAsync());
        BackCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
    }

    public IReadOnlyList<string> RelationshipOptions { get; }

    public string FirstNameLabel { get; }
    public string LastNameLabel { get; }
    public string RelationshipLabel { get; }
    public string DobLabel { get; }
    public string PhoneLabel { get; }
    public string EmailLabel { get; }
    public string SaveLabel { get; }
    public string RemoveLabel { get; }

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (EqualityComparer<string>.Default.Equals(_firstName, value))
                return;
            SetProperty(ref _firstName, value);
            RefreshSave();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            if (EqualityComparer<string>.Default.Equals(_lastName, value))
                return;
            SetProperty(ref _lastName, value);
            RefreshSave();
        }
    }

    public string? SelectedRelationship
    {
        get => _selectedRelationship;
        set
        {
            if (EqualityComparer<string?>.Default.Equals(_selectedRelationship, value))
                return;
            SetProperty(ref _selectedRelationship, value);
            RefreshSave();
        }
    }

    public bool HasDateOfBirth
    {
        get => _hasDateOfBirth;
        set => SetProperty(ref _hasDateOfBirth, value);
    }

    public DateTime DobDate
    {
        get => _dobDate;
        set => SetProperty(ref _dobDate, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool CanSave => !IsBusy
        && _memberId != Guid.Empty
        && !string.IsNullOrWhiteSpace(FirstName)
        && !string.IsNullOrWhiteSpace(LastName)
        && !string.IsNullOrWhiteSpace(SelectedRelationship);

    public ICommand SaveCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand BackCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (TryGetQueryGuid(query, "memberId", out var id))
            _memberId = id;
    }

    private void RefreshSave() => RaiseCanExecuteChanged(SaveCommand);

    public async Task LoadAsync()
    {
        if (_memberId == Guid.Empty)
        {
            ErrorMessage = T("FamilyDetailFailed");
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        RefreshSave();
        try
        {
            var response = await _family.GetMyFamilyMemberAsync(_memberId, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? T("FamilyDetailFailed");
                return;
            }

            var d = response.Data;
            _linkedPatientId = d.LinkedPatientId;
            FirstName = d.FirstName;
            LastName = d.LastName;
            SelectedRelationship = RelationshipOptions.Contains(d.Relationship) ? d.Relationship : "Other";
            Phone = d.PhoneNumber ?? string.Empty;
            Email = d.Email ?? string.Empty;
            if (d.DateOfBirth is { } dob)
            {
                HasDateOfBirth = true;
                DobDate = dob.Date;
            }
            else
            {
                HasDateOfBirth = false;
                DobDate = DateTime.Today.AddYears(-10);
            }
        }
        finally
        {
            IsBusy = false;
            RefreshSave();
        }
    }

    private async Task SaveAsync()
    {
        if (!CanSave) return;
        ErrorMessage = null;
        IsBusy = true;
        RefreshSave();
        try
        {
            var dob = HasDateOfBirth ? DobDate.Date : (DateTime?)null;
            var res = await _family.UpdateFamilyMemberAsync(
                _memberId,
                FirstName.Trim(),
                LastName.Trim(),
                SelectedRelationship!.Trim(),
                dob,
                string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                _linkedPatientId,
                CancellationToken.None).ConfigureAwait(false);
            if (!res.IsSuccess)
                ErrorMessage = res.ErrorMessage ?? T("FamilySaveFailed");
            else
                await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
            RefreshSave();
        }
    }

    private async Task RemoveAsync()
    {
        if (_memberId == Guid.Empty) return;
        IsBusy = true;
        try
        {
            var res = await _family.RemoveFamilyMemberAsync(_memberId, CancellationToken.None).ConfigureAwait(false);
            if (!res.IsSuccess)
                ErrorMessage = res.ErrorMessage ?? T("FamilyRemoveFailed");
            else
                await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
