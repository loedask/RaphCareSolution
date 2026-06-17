using RaphCare.Mobile.Core.Features.Family.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Family.Views;

public partial class FamilyMembersPage : ContentPage
{
    public FamilyMembersPage() : this(MobileServiceHub.GetRequiredService<FamilyMembersViewModel>()) { }

    public FamilyMembersPage(FamilyMembersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FamilyMembersViewModel vm)
            await vm.LoadAsync();
    }
}
