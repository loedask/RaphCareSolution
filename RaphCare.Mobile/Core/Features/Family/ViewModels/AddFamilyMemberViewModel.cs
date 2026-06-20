using System.Collections.Generic;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Family.ViewModels;

public sealed class AddFamilyMemberViewModel : BaseViewModel
{
    private readonly IPatientFamilyMembersService _family;
    private string? _errorMessage;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string? _selectedRelationship;
    private bool _hasDateOfBirth;
    private DateTime _dobDate = DateTime.Today.AddYears(-10);
    private string _phone = string.Empty;
    private string _email = string.Empty;

    public AddFamilyMemberViewModel(IPatientFamilyMembersService family)
    {
        _family = family ?? throw new ArgumentNullException(nameof(family));
        Title = T("FamilyAddTitle");
        RelationshipOptions = ["Child", "Spouse", "Parent", "Sibling", "Other"];
        _selectedRelationship = RelationshipOptions[0];

        FirstNameLabel = T("FamilyFieldFirstName");
        LastNameLabel = T("FamilyFieldLastName");
        RelationshipLabel = T("FamilyFieldRelationship");
        DobLabel = T("FamilyFieldDob");
        DobHint = T("FamilyDobHint");
        PhoneLabel = T("FamilyFieldPhone");
        EmailLabel = T("FamilyFieldEmail");
        SubmitLabel = T("FamilySave");
        CancelLabel = T("FamilyCancel");

        SubmitCommand = new Command(async () => await SubmitAsync(), () => CanSubmit);
        CancelCommand = new Command(async () => await SafeShellNavigator.GoToAsync(".."));
    }

    public IReadOnlyList<string> RelationshipOptions { get; }

    public string FirstNameLabel { get; }
    public string LastNameLabel { get; }
    public string RelationshipLabel { get; }
    public string DobLabel { get; }
    public string DobHint { get; }
    public string PhoneLabel { get; }
    public string EmailLabel { get; }
    public string SubmitLabel { get; }
    public string CancelLabel { get; }

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (EqualityComparer<string>.Default.Equals(_firstName, value))
                return;
            SetProperty(ref _firstName, value);
            RefreshSubmit();
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
            RefreshSubmit();
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
            RefreshSubmit();
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

    private bool CanSubmit => !IsBusy
        && !string.IsNullOrWhiteSpace(FirstName)
        && !string.IsNullOrWhiteSpace(LastName)
        && !string.IsNullOrWhiteSpace(SelectedRelationship);

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }

    private void RefreshSubmit()
    {
        if (SubmitCommand is Command c)
            c.ChangeCanExecute();
    }

    private async Task SubmitAsync()
    {
        if (IsBusy || !CanSubmit) return;
        ErrorMessage = null;
        IsBusy = true;
        RefreshSubmit();
        try
        {
            var dob = HasDateOfBirth ? DobDate.Date : (DateTime?)null;
            var res = await _family.AddFamilyMemberAsync(
                FirstName.Trim(),
                LastName.Trim(),
                SelectedRelationship!.Trim(),
                dob,
                string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                CancellationToken.None).ConfigureAwait(false);
            if (!res.IsSuccess)
            {
                ErrorMessage = res.ErrorMessage ?? T("FamilySaveFailed");
                return;
            }

            await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
            RefreshSubmit();
        }
    }
}
