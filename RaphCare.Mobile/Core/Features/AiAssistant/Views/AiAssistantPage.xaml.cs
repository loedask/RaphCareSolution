using RaphCare.Mobile.Core.Features.AiAssistant.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.AiAssistant.Views;

public partial class AiAssistantPage : ContentPage
{
    public AiAssistantPage() : this(MobileServiceHub.GetRequiredService<AiAssistantViewModel>()) { }

    public AiAssistantPage(AiAssistantViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
