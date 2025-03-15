using FlashbackMonitor.Services;
using ReactiveUI;
using System.Threading.Tasks;

namespace FlashbackMonitor.ViewModels
{
    public class ThreadsListUserControlViewModel : ViewModelBase
    {
        private readonly IFlashbackService _flashbackService;

        public string _forumUrl;
        public string ForumUrl
        {
            get => _forumUrl;
            set => this.RaiseAndSetIfChanged(ref _forumUrl, value);
        }

        private ThreadListPage _threadListPage;
        public ThreadListPage ThreadListPage
        {
            get => _threadListPage;
            set => this.RaiseAndSetIfChanged(ref _threadListPage, value);
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private string _loadingText;
        public string LoadingText
        {
            get => _loadingText;
            set => this.RaiseAndSetIfChanged(ref _loadingText, value);
        }

        public int MessageLoadingCount { get; set; } = 1;

        public ThreadsListUserControlViewModel() { }

        public ThreadsListUserControlViewModel(IFlashbackService flashbackService, string forumUrl)
        {
            _flashbackService = flashbackService;
            ForumUrl = forumUrl;
        }

        public async Task InitializeAsync()
        {
            MessageLoadingCount = 1;

        load:
            try
            {
                IsLoading = true;

                LoadingText = $"Hämtar data... (försök {MessageLoadingCount})";

                ThreadListPage = await _flashbackService.GetThreadListPageAsync(ForumUrl);

                IsLoading = false;
            }
            catch
            {
                await Task.Delay(10000);
                MessageLoadingCount++;
                goto load;
            }
        }
    }
}