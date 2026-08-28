using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Help &amp; support (concept <c>HelpSupport.tsx</c>).</summary>
public sealed class HelpSupportViewModel : BaseViewModel
{
    private readonly IPatientSupportService _support;
    private string _callSubtitle = string.Empty;
    private string _emailSubtitle = string.Empty;
    private string? _phoneUri;
    private string? _mailUri;

    public HelpSupportViewModel(IPatientSupportService support)
    {
        _support = support ?? throw new ArgumentNullException(nameof(support));
        Title = T("HelpSupportTitle");
        ContactSectionTitle = T("HelpContactSection");
        TipsSectionTitle = T("HelpTipsSection");
        Tip1 = T("HelpTip1");
        Tip2 = T("HelpTip2");
        Tip3 = T("HelpTip3");
        Tip4 = T("HelpTip4");
        FaqCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.HelpFaq));
        MessageCommand = new Command(async () => await SafeShellNavigator.GoToAsync(AppNavigator.SupportMessage));
        CallCommand = new Command(async () => await OpenUriAsync(_phoneUri));
        EmailCommand = new Command(async () => await OpenUriAsync(_mailUri));
        FaqTitle = T("HelpFaq");
        FaqSubtitle = T("HelpFaqSubtitle");
        MessageTitle = T("HelpSendMessage");
        MessageSubtitle = T("HelpSendMessageSubtitle");
        CallTitle = T("HelpCallSupport");
        EmailTitle = T("HelpEmailSupport");
        _callSubtitle = T("HelpCallSubtitle");
        _emailSubtitle = T("HelpEmailSubtitle");
        _phoneUri = T("HelpSupportPhoneUri");
        _mailUri = T("HelpSupportMailUri");
        AppVersionLabel = $"{T("ProfileAppName")} v{AppInfo.Current.VersionString}";
    }

    public string FaqTitle { get; }
    public string FaqSubtitle { get; }
    public string MessageTitle { get; }
    public string MessageSubtitle { get; }
    public string CallTitle { get; }
    public string EmailTitle { get; }

    public string CallSubtitle
    {
        get => _callSubtitle;
        private set => SetProperty(ref _callSubtitle, value);
    }

    public string EmailSubtitle
    {
        get => _emailSubtitle;
        private set => SetProperty(ref _emailSubtitle, value);
    }

    public string ContactSectionTitle { get; }
    public string TipsSectionTitle { get; }
    public string Tip1 { get; }
    public string Tip2 { get; }
    public string Tip3 { get; }
    public string Tip4 { get; }
    public string AppVersionLabel { get; }

    public ICommand FaqCommand { get; }
    public ICommand MessageCommand { get; }
    public ICommand CallCommand { get; }
    public ICommand EmailCommand { get; }

    public async Task LoadAsync()
    {
        var response = await _support.GetContentAsync(CancellationToken.None).ConfigureAwait(false);
        if (!response.IsSuccess || response.Data is null)
            return;

        var data = response.Data;
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (!string.IsNullOrWhiteSpace(data.SupportPhoneDisplay))
                CallSubtitle = data.SupportPhoneDisplay;
            if (!string.IsNullOrWhiteSpace(data.SupportEmail))
                EmailSubtitle = data.SupportEmail;
            if (!string.IsNullOrWhiteSpace(data.SupportPhoneE164))
                _phoneUri = $"tel:{data.SupportPhoneE164.Trim()}";
            if (!string.IsNullOrWhiteSpace(data.SupportEmail))
                _mailUri = $"mailto:{data.SupportEmail.Trim()}";
        });
    }

    private static async Task OpenUriAsync(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            return;
        try
        {
            await Launcher.Default.OpenAsync(new Uri(uri));
        }
        catch
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync(
                    T("HelpSupportTitle"),
                    T("HelpLauncherFailed"),
                    T("CommonOk")));
        }
    }
}
