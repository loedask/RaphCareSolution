using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.AiAssistant;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.AiAssistant.ViewModels;

/// <summary>Patient AI assistant (concept <c>/ai-assistant</c>).</summary>
public sealed class AiAssistantViewModel : BaseViewModel
{
    private readonly IPatientAiAssistantService _assistant;
    private string _draftMessage = string.Empty;
    private string _replyText = string.Empty;
    private string _disclaimerText = string.Empty;
    private string? _errorMessage;

    public AiAssistantViewModel(IPatientAiAssistantService assistant)
    {
        _assistant = assistant ?? throw new ArgumentNullException(nameof(assistant));
        Title = AppResources.T("AiAssistantTitle");
        SendCommand = new Command(async () => await SendAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(DraftMessage));
        DraftMessage = string.Empty;
    }

    public string IntroText => AppResources.T("AiAssistantIntro");
    public string PlaceholderText => AppResources.T("AiAssistantPlaceholder");
    public string SendButtonText => AppResources.T("AiAssistantSend");
    public string ReplyHeading => AppResources.T("AiAssistantReplyHeading");
    public string EmptyReplyText => AppResources.T("AiAssistantEmptyReply");

    public string DraftMessage
    {
        get => _draftMessage;
        set
        {
            SetProperty(ref _draftMessage, value);
            ((Command)SendCommand).ChangeCanExecute();
        }
    }

    public string ReplyText
    {
        get => _replyText;
        set => SetProperty(ref _replyText, value);
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

    public bool HasReply => !string.IsNullOrWhiteSpace(ReplyText);

    public ICommand SendCommand { get; }

    private async Task SendAsync()
    {
        if (IsBusy) return;
        var text = DraftMessage.Trim();
        if (text.Length == 0) return;

        ErrorMessage = null;
        IsBusy = true;
        ((Command)SendCommand).ChangeCanExecute();
        try
        {
            var response = await _assistant.SendMessageAsync(
                new SendMyPatientAssistantMessageRequest { Message = text },
                CancellationToken.None).ConfigureAwait(false);

            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.ErrorMessage ?? AppResources.T("AiAssistantSendFailed");
                return;
            }

            ReplyText = response.Data.Reply;
            DisclaimerText = response.Data.MedicalDisclaimer;
            OnPropertyChanged(nameof(HasReply));
        }
        finally
        {
            IsBusy = false;
            ((Command)SendCommand).ChangeCanExecute();
        }
    }
}
