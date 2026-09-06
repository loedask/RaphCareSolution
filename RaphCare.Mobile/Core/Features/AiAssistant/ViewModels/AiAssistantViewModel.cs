using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.AiAssistant;
using RaphCare.Mobile.Core.Common.AiAssistant;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Core.Features.AiAssistant.Models;

namespace RaphCare.Mobile.Core.Features.AiAssistant.ViewModels;

/// <summary>Patient AI assistant chat thread (concept <c>/ai-assistant</c>).</summary>
public sealed class AiAssistantViewModel : BaseViewModel
{
    private readonly IPatientAiAssistantService _assistant;
    private string _draftMessage = string.Empty;
    private string _disclaimerText = string.Empty;
    private string? _errorMessage;

    public AiAssistantViewModel(IPatientAiAssistantService assistant)
    {
        _assistant = assistant ?? throw new ArgumentNullException(nameof(assistant));
        Title = T("AiAssistantTitle");

        IntroText = T("AiAssistantIntro");
        PlaceholderText = T("AiAssistantPlaceholder");
        SendButtonText = T("AiAssistantSend");
        DisclaimerFallbackText = T("AiAssistantDisclaimerFallback");

        Messages = [];
        Messages.CollectionChanged += OnMessagesCollectionChanged;

        SendCommand = new Command(async () => await SendAsync(), CanSend);
        SeedGreeting();
    }

    /// <summary>Raised when the message list grows so the page can scroll to the latest bubble.</summary>
    public event EventHandler? MessagesChanged;

    public string IntroText { get; }
    public string PlaceholderText { get; }
    public string SendButtonText { get; }
    public string DisclaimerFallbackText { get; }

    public ObservableCollection<AiAssistantChatMessage> Messages { get; }

    public string DraftMessage
    {
        get => _draftMessage;
        set
        {
            SetProperty(ref _draftMessage, value);
            RaiseCanExecuteChanged(SendCommand);
        }
    }

    public string DisclaimerText
    {
        get => _disclaimerText;
        set => SetProperty(ref _disclaimerText, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasDisclaimer => !string.IsNullOrWhiteSpace(DisclaimerText);

    public ICommand SendCommand { get; }

    /// <summary>Clears the thread (for example after sign-out) and reseeds the greeting.</summary>
    public void ResetConversation()
    {
        ErrorMessage = null;
        DraftMessage = string.Empty;
        DisclaimerText = string.Empty;
        OnPropertyChanged(nameof(HasDisclaimer));
        Messages.Clear();
        SeedGreeting();
    }

    private void SeedGreeting()
    {
        if (!AiAssistantChatSessionRules.NeedsGreetingSeed(Messages.Count))
            return;

        Messages.Add(new AiAssistantChatMessage(isFromUser: false, T("AiAssistantGreeting")));
    }

    private bool CanSend() =>
        !IsBusy && AiAssistantChatSessionRules.TryNormalizeOutgoing(DraftMessage) is not null;

    private async Task SendAsync()
    {
        if (IsBusy)
            return;

        var text = AiAssistantChatSessionRules.TryNormalizeOutgoing(DraftMessage);
        if (text is null)
            return;

        ErrorMessage = null;
        IsBusy = true;
        RaiseCanExecuteChanged(SendCommand);

        List<PatientAssistantPriorMessage> priorMessages = [];
        await RunOnMainThreadAsync(() =>
        {
            var prior = AiAssistantChatSessionRules.SelectPriorForRequest(
                Messages.Select(m => (m.IsFromUser, m.Text)));
            priorMessages = prior
                .Select(m => new PatientAssistantPriorMessage
                {
                    Role = m.IsFromUser ? "user" : "assistant",
                    Content = m.Text,
                })
                .ToList();

            Messages.Add(new AiAssistantChatMessage(isFromUser: true, text));
            DraftMessage = string.Empty;
        }).ConfigureAwait(false);

        try
        {
            var request = new SendMyPatientAssistantMessageRequest
            {
                Message = text,
                PriorMessages = priorMessages,
            };

            var response = await _assistant.SendMessageAsync(request, CancellationToken.None)
                .ConfigureAwait(false);

            if (!response.IsSuccess || response.Data is null)
            {
                await RunOnMainThreadAsync(() =>
                {
                    ErrorMessage = response.ErrorMessage ?? T("AiAssistantSendFailed");
                }).ConfigureAwait(false);
                return;
            }

            var reply = response.Data.Reply?.Trim() ?? string.Empty;
            var disclaimer = string.IsNullOrWhiteSpace(response.Data.MedicalDisclaimer)
                ? DisclaimerFallbackText
                : response.Data.MedicalDisclaimer.Trim();

            await RunOnMainThreadAsync(() =>
            {
                if (reply.Length > 0)
                    Messages.Add(new AiAssistantChatMessage(isFromUser: false, reply));
                else
                    ErrorMessage = T("AiAssistantSendFailed");

                DisclaimerText = disclaimer;
                OnPropertyChanged(nameof(HasDisclaimer));
            }).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged(SendCommand);
        }
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        MessagesChanged?.Invoke(this, EventArgs.Empty);
}
