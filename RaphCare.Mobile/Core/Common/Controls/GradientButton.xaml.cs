namespace RaphCare.Mobile.Core.Shared.Controls;

public partial class GradientButton : Button
{
    public GradientButton()
    {
        InitializeComponent();
    }

    private void GradientButton_OnClicked(object? sender, EventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }
}
