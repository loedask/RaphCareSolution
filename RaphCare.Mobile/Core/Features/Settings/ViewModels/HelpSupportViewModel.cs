using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>Help &amp; support (concept <c>HelpSupport.tsx</c>).</summary>
public sealed class HelpSupportViewModel : BaseViewModel
{
    public HelpSupportViewModel()
    {
        Title = AppResources.T("HelpSupportTitle");
        FaqCommand = new Command(async () => await ToastComingSoonAsync(AppResources.T("HelpFaq")));
        MessageCommand = new Command(async () => await ToastComingSoonAsync(AppResources.T("HelpSendMessage")));
        CallCommand = new Command(async () => await OpenUriAsync(AppResources.T("HelpSupportPhoneUri")));
        EmailCommand = new Command(async () => await OpenUriAsync(AppResources.T("HelpSupportMailUri")));

        FaqTitle = AppResources.T("HelpFaq");
        FaqSubtitle = AppResources.T("HelpFaqSubtitle");
        MessageTitle = AppResources.T("HelpSendMessage");
        MessageSubtitle = AppResources.T("HelpSendMessageSubtitle");
        CallTitle = AppResources.T("HelpCallSupport");
        CallSubtitle = AppResources.T("HelpCallSubtitle");
        EmailTitle = AppResources.T("HelpEmailSupport");
        EmailSubtitle = AppResources.T("HelpEmailSubtitle");
    }

    public string FaqTitle { get; }
    public string FaqSubtitle { get; }
    public string MessageTitle { get; }
    public string MessageSubtitle { get; }
    public string CallTitle { get; }
    public string CallSubtitle { get; }
    public string EmailTitle { get; }
    public string EmailSubtitle { get; }

    public string ContactSectionTitle => AppResources.T("HelpContactSection");
    public string TipsSectionTitle => AppResources.T("HelpTipsSection");
    public string Tip1 => AppResources.T("HelpTip1");
    public string Tip2 => AppResources.T("HelpTip2");
    public string Tip3 => AppResources.T("HelpTip3");
    public string Tip4 => AppResources.T("HelpTip4");
    public string AppVersionLabel => $"{AppResources.T("ProfileAppName")} v{AppInfo.Current.VersionString}";

    public ICommand FaqCommand { get; }
    public ICommand MessageCommand { get; }
    public ICommand CallCommand { get; }
    public ICommand EmailCommand { get; }

    private static async Task ToastComingSoonAsync(string title) =>
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(title, AppResources.T("ProfileFeatureComingSoon"), AppResources.T("CommonOk")));

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
                    AppResources.T("HelpSupportTitle"),
                    AppResources.T("HelpLauncherFailed"),
                    AppResources.T("CommonOk")));
        }
    }
}
