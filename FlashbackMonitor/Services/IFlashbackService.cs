using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashbackMonitor.Services
{
    public interface IFlashbackService
    {
        Task<IEnumerable<FlashbackDataItem>> GetFlashbackDataAsync();
        Task<TopicPage> GetTopicPageAsync(string topicUrl);
        Task<ThreadListPage> GetThreadListPageAsync(string forumUrl);
    }
}