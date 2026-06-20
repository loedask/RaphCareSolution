using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Help &amp; support (concept <c>HelpSupport.tsx</c>).</summary>
public sealed class HelpSupportViewModel : BaseViewModel
{
    public HelpSupportViewModel()
    {
        Title = T("HelpSupportTitle");
        ContactSectionTitle = T("HelpContactSection");
        TipsSectionTitle = T("HelpTipsSection");
        Tip1 = T("HelpTip1");
        Tip2 = T("HelpTip2");
        Tip3 = T("HelpTip3");
        Tip4 = T("HelpTip4");
        FaqCommand = new Command(async () => await ToastComingSoonAsync(T("HelpFaq")));
        MessageCommand = new Command(async () => await ToastComingSoonAsync(T("HelpSendMessage")));
        CallCommand = new Command(async () => await OpenUriAsync(T("HelpSupportPhoneUri")));
        EmailCommand = new Command(async () => await OpenUriAsync(T("HelpSupportMailUri")));
        FaqTitle = T("HelpFaq");
        FaqSubtitle = T("HelpFaqSubtitle");
        MessageTitle = T("HelpSendMessage");
        MessageSubtitle = T("HelpSendMessageSubtitle");
        CallTitle = T("HelpCallSupport");
        CallSubtitle = T("HelpCallSubtitle");
        EmailTitle = T("HelpEmailSupport");
        EmailSubtitle = T("HelpEmailSubtitle");
        AppVersionLabel = $"{T("ProfileAppName")} v{AppInfo.Current.VersionString}";
    }

    public string FaqTitle { get; }
    public string FaqSubtitle { get; }
    public string MessageTitle { get; }
    public string MessageSubtitle { get; }
    public string CallTitle { get; }
    public string CallSubtitle { get; }
    public string EmailTitle { get; }
    public string EmailSubtitle { get; }

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

    private static async Task ToastComingSoonAsync(string title) =>
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(title, T("ProfileFeatureComingSoon"), T("CommonOk")));

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
