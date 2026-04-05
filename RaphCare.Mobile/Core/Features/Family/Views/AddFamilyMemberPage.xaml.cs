using RaphCare.Mobile.Core.Features.Family.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Family.Views;

public partial class AddFamilyMemberPage : ContentPage
{
    public AddFamilyMemberPage() : this(MobileServiceHub.GetRequiredService<AddFamilyMemberViewModel>()) { }

    public AddFamilyMemberPage(AddFamilyMemberViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
