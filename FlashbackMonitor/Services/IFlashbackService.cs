using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashbackMonitor.Services
{
    public interface IFlashbackService
    {
        Task<List<FlashbackDataItem>> GetFlashbackDataAsync();
    }
}
