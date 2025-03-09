using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FlashbackMonitor.Views;

namespace FlashbackMonitor
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
            {
                var mainWindow = new MainWindow
                {
                    CanResize = false,
                    Width = 900,
                    Height = 900,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                mainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}