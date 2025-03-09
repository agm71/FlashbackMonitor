using FlashbackMonitor.ViewModels;
using System.Threading.Tasks;

namespace FlashbackMonitor.Services
{
    public interface ISettingsService
    {
        Task<Settings> GetSettingsAsync();
        Settings GetSettings();
        Task SaveSettingsAsync(MainWindowViewModel viewModel);
        void SaveSettings(MainWindowViewModel viewModel);
    }
}
