using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Family.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Family.Views;

public partial class FamilyMemberDetailPage : ContentPage, IQueryAttributable
{
    public FamilyMemberDetailPage() : this(MobileServiceHub.GetRequiredService<FamilyMemberDetailViewModel>()) { }

    public FamilyMemberDetailPage(FamilyMemberDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is FamilyMemberDetailViewModel vm)
            vm.ApplyQueryAttributes(query);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FamilyMemberDetailViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
