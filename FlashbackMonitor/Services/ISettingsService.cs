using FlashbackMonitor.ViewModels;
using System.Threading.Tasks;

namespace FlashbackMonitor.Services
{
    public interface ISettingsService
    {
        Task<Settings> GetSettingsAsync();
        Task SaveSettingsAsync(MainWindowViewModel viewModel);
    }
}
