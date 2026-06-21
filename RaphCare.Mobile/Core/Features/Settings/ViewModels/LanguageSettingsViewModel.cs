using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Common.Services.Localization;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>App language picker (concept <c>Language.tsx</c>).</summary>
public sealed class LanguageSettingsViewModel : BaseViewModel
{
    public LanguageSettingsViewModel()
    {
        Title = T("LanguageSettingsTitle");
        Languages = AppLanguagePreference.CreateSelectableList();
        SelectCommand = new Command<LanguageOption>(option =>
        {
            if (option is null) return;
            AppLanguagePreference.Apply(option.Code);
            AppLanguagePreference.MarkSelection(Languages, option.Code);
            MainThread.BeginInvokeOnMainThread(async () =>
                await Shell.Current.DisplayAlertAsync(
                    Title,
                    string.Format(T("LanguageSettingsUpdatedFormat"), option.Native),
                    T("CommonOk")));
        });
    }

    public ObservableCollection<LanguageOption> Languages { get; }
    public ICommand SelectCommand { get; }
}
