using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
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
            ((Command)SendCommand).ChangeCanExecute();
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            SetProperty(ref _message, value);
            ((Command)SendCommand).ChangeCanExecute();
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
        ((Command)SendCommand).ChangeCanExecute();
        try
        {
            var response = await _support.SubmitMessageAsync(new SubmitPatientSupportMessageRequest
            {
                Subject = Subject.Trim(),
                Message = Message.Trim()
            }, CancellationToken.None).ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlertAsync(Title, T("SupportMessageFailed"), T("CommonOk")));
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlertAsync(T("SupportMessageSentTitle"), T("SupportMessageSentBody"), T("CommonOk"));
                await SafeShellNavigator.GoToAsync("..");
            });
        }
        finally
        {
            IsBusy = false;
            ((Command)SendCommand).ChangeCanExecute();
        }
    }
}
