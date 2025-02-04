using Avalonia;
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
                var mainWindow = new MainWindow();
                mainWindow.CanResize = false;
                mainWindow.Width = 900;
                mainWindow.Height = 700;
                mainWindow.Show();
#pragma warning disable CS4014
                InitializeDataAsync(mainWindow);
#pragma warning restore CS4014
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task InitializeDataAsync(MainWindow mainWindow)
        {
            var flashbackService = new FlashbackService();  
            var settingsService = new SettingsService();

            mainWindow.DataContext = await MainWindowViewModel.CreateAsync(flashbackService, settingsService);
        }
    }
}