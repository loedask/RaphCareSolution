using System.Windows.Input;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>
/// Legacy email verification screen. Registration and sign-in now collect codes inline; this page supports resend and redirect to sign-in.
/// </summary>
public class VerifyEmailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IEmailAuthService _emailAuthService;

    private string _email = string.Empty;
    private string? _errorMessage;
    private string? _statusMessage;

    public VerifyEmailViewModel(IEmailAuthService emailAuthService)
    {
        _emailAuthService = emailAuthService ?? throw new ArgumentNullException(nameof(emailAuthService));
        Title = T("VerifyEmailPageTitle");
        Headline = T("VerifyEmailTitle");
        Subtitle = T("VerifyEmailSubtitle");
        SignInButtonText = T("VerifyEmailSignIn");
        ResendButtonText = T("VerifyEmailResend");

        SignInCommand = new Command(async () => await SignInAsync(), () => !IsBusy);
        ResendCommand = new Command(async () => await ResendAsync(), () => !IsBusy);
    }

    public string Headline { get; }
    public string Subtitle { get; }
    public string SignInButtonText { get; }
    public string ResendButtonText { get; }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value ?? string.Empty);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SignInCommand { get; }
    public ICommand ResendCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("email", out var email) && email is string emailText)
            Email = emailText;
    }

    private async Task SignInAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        StatusMessage = null;
        IsBusy = true;
        try
        {
            await SafeShellNavigator.GoToAsync(AppNavigator.SignIn).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResendAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;
        StatusMessage = null;

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = T("RegisterEmailErrorEmail");
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _emailAuthService
                .SendEmailVerificationAsync(Email.Trim(), CancellationToken.None)
                .ConfigureAwait(false);

            if (response.IsSuccess)
                StatusMessage = T("RegisterEmailCodeSent");
            else
                ErrorMessage = response.ErrorMessage ?? T("RegisterEmailSendCodeFailed");
        }
        catch (Exception)
        {
            ErrorMessage = T("RegisterEmailSendCodeFailed");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
