using Microsoft.Extensions.DependencyInjection;

namespace RaphCare.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = MauiProgram.ServiceProvider!.GetRequiredService<AppShell>();
            return new Window(shell) { Title = "RaphCare" };
        }
    }
}
