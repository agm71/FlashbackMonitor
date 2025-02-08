using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FlashbackMonitor.Services;
using FlashbackMonitor.ViewModels;
using FlashbackMonitor.Views;
using System.Threading.Tasks;

namespace FlashbackMonitor
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow
                {
                    CanResize = false,
                    Width = 900,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                mainWindow.Show();

#pragma warning disable CS4014
                InitializeDataAsync(mainWindow);
#pragma warning restore CS4014
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static async Task InitializeDataAsync(MainWindow mainWindow)
        {
            var flashbackService = new FlashbackService();  
            var settingsService = new SettingsService();

            mainWindow.DataContext = await MainWindowViewModel.CreateAsync(flashbackService, settingsService);
        }
    }
}