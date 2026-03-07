using System.Windows.Input;

namespace RaphCare.Mobile.Shared.Views;

[QueryProperty(nameof(FeatureName), "featureName")]
public partial class UnderConstructionPage : ContentPage
{
    public static readonly BindableProperty FeatureNameProperty =
        BindableProperty.Create(nameof(FeatureName), typeof(string), typeof(UnderConstructionPage), string.Empty,
            propertyChanged: (b, _, newVal) => ((UnderConstructionPage)b).UpdateFeatureLabel());

    public static readonly BindableProperty GoBackCommandProperty =
        BindableProperty.Create(nameof(GoBackCommand), typeof(ICommand), typeof(UnderConstructionPage), null);

    public string FeatureName { get => (string)GetValue(FeatureNameProperty); set => SetValue(FeatureNameProperty, value); }
    public ICommand? GoBackCommand { get => (ICommand?)GetValue(GoBackCommandProperty); set => SetValue(GoBackCommandProperty, value); }

    public UnderConstructionPage()
    {
        InitializeComponent();
        UpdateFeatureLabel();
        GoBackCommand = new Command(async () =>
        {
            var shell = Shell.Current;
            if (shell?.Navigation?.NavigationStack?.Count > 1)
                await shell.GoToAsync("..");
            else if (shell != null)
                await shell.GoToAsync("//HomePage");
        });
    }

    private void UpdateFeatureLabel()
    {
        if (FeatureLabel == null) return;
        if (!string.IsNullOrEmpty(FeatureName))
        {
            FeatureLabel.Text = $"We're working on {FeatureName}.";
            FeatureLabel.IsVisible = true;
        }
        else
            FeatureLabel.IsVisible = false;
    }
}
