using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Support;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

/// <summary>FAQ list loaded from <c>api/patient/support</c>.</summary>
public sealed class HelpFaqViewModel : BaseViewModel
{
    private readonly IPatientSupportService _support;

    public HelpFaqViewModel(IPatientSupportService support)
    {
        _support = support ?? throw new ArgumentNullException(nameof(support));
        Title = T("HelpFaq");
        EmptyText = T("HelpFaqEmpty");
        Items = new ObservableCollection<PatientSupportFaqItemViewModel>();
    }

    public string EmptyText { get; }

    public ObservableCollection<PatientSupportFaqItemViewModel> Items { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var response = await _support.GetContentAsync(CancellationToken.None).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Items.Clear();
                if (!response.IsSuccess || response.Data?.Faq is not { Count: > 0 } faq)
                    return;

                foreach (var item in faq)
                    Items.Add(item);
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task OpenItemAsync(PatientSupportFaqItemViewModel? item)
    {
        if (item is null)
            return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.DisplayAlertAsync(item.Question, item.Answer, T("CommonOk")));
    }
}
