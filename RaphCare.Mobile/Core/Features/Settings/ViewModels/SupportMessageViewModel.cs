using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Support;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Submit a support message via <c>api/patient/support/messages</c>.</summary>
public sealed class SupportMessageViewModel : BaseViewModel
{
    private readonly IPatientSupportService _support;
    private string _subject = string.Empty;
    private string _message = string.Empty;

    public SupportMessageViewModel(IPatientSupportService support)
    {
        _support = support ?? throw new ArgumentNullException(nameof(support));
        Title = T("HelpSendMessage");
        SubjectLabel = T("SupportMessageSubjectLabel");
        MessageLabel = T("SupportMessageBodyLabel");
        SendButtonText = T("SupportMessageSend");
        SendCommand = new Command(async () => await SendAsync(), () => CanSend);
    }

    public string SubjectLabel { get; }
    public string MessageLabel { get; }
    public string SendButtonText { get; }

    public string Subject
    {
        get => _subject;
        set
        {
            SetProperty(ref _subject, value);
            RaiseCanExecuteChanged(SendCommand);
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            SetProperty(ref _message, value);
            RaiseCanExecuteChanged(SendCommand);
        }
    }

    private bool CanSend =>
        !IsBusy
        && !string.IsNullOrWhiteSpace(Subject)
        && !string.IsNullOrWhiteSpace(Message);

    public ICommand SendCommand { get; }

    private async Task SendAsync()
    {
        IsBusy = true;
        RaiseCanExecuteChanged(SendCommand);
        try
        {
            var response = await _support.SubmitMessageAsync(new SubmitPatientSupportMessageRequest
            {
                Subject = Subject.Trim(),
                Message = Message.Trim()
            }, CancellationToken.None).ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                await DisplayAlertSafeAsync(Title, T("SupportMessageFailed"), T("CommonOk"));
                return;
            }

            await DisplayAlertSafeAsync(T("SupportMessageSentTitle"), T("SupportMessageSentBody"), T("CommonOk"));
            await SafeShellNavigator.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged(SendCommand);
        }
    }
}
