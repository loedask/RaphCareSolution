using System.Collections.ObjectModel;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Family;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Family.ViewModels;

public sealed class FamilyMembersViewModel : BaseViewModel
{
    private readonly IPatientFamilyMembersService _family;
    private string? _errorMessage;

    public FamilyMembersViewModel(IPatientFamilyMembersService family)
    {
        _family = family ?? throw new ArgumentNullException(nameof(family));
        Title = AppResources.T("FamilyTitle");
        RefreshButtonText = AppResources.T("FamilyRefresh");
        AddButtonText = AppResources.T("FamilyAddMember");
        EmptyStateText = AppResources.T("FamilyEmpty");
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowEmpty));

        RefreshCommand = new Command(async () => await LoadAsync());
        AddCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.AddFamilyMember));
        OpenDetailCommand = new Command<Guid>(async id =>
            await SafeShellNavigator.GoToAsync($"{AppNavigator.FamilyMemberDetail}?memberId={id}"));
    }

    public ObservableCollection<PatientFamilyMemberViewModel> Items { get; } = new();

    public string RefreshButtonText { get; }
    public string AddButtonText { get; }
    public string EmptyStateText { get; }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowEmpty => !IsBusy && Items.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand OpenDetailCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        IsBusy = true;
        OnPropertyChanged(nameof(ShowEmpty));
        try
        {
            var response = await _family.GetMyFamilyMembersAsync(CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? AppResources.T("FamilyLoadFailed");
                Items.Clear();
                return;
            }

            Items.Clear();
            foreach (var m in response.Data)
                Items.Add(m);
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(ShowEmpty));
        }
    }
}
