using RaphCare.Mobile.Core.Features.AiAssistant.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.AiAssistant.Views;

public partial class AiAssistantPage : ContentPage
{
    private AiAssistantViewModel? _viewModel;

    public AiAssistantPage() : this(MobileServiceHub.GetRequiredService<AiAssistantViewModel>()) { }

    public AiAssistantPage(AiAssistantViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel is null)
            return;

        _viewModel.MessagesChanged += OnMessagesChanged;
        ScrollToLatest(animate: false);
    }

    protected override void OnDisappearing()
    {
        if (_viewModel is not null)
            _viewModel.MessagesChanged -= OnMessagesChanged;

        base.OnDisappearing();
    }

    private void OnMessagesChanged(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Let CollectionView realize the new cell before ScrollTo (Android process kill otherwise).
            await Task.Delay(50).ConfigureAwait(true);
            ScrollToLatest(animate: true);
        });

    private void ScrollToLatest(bool animate)
    {
        if (_viewModel is null || _viewModel.Messages.Count == 0)
            return;

        try
        {
            var index = _viewModel.Messages.Count - 1;
            MessagesList.ScrollTo(index, position: ScrollToPosition.End, animate: animate);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AiAssistant ScrollTo skipped: {ex.Message}");
        }
    }
}
