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

        private bool _showRetryButton;
        public bool ShowRetryButton
        {
            get => _showRetryButton;
            set => this.RaiseAndSetIfChanged(ref _showRetryButton, value);
        }

        private bool _showErrorMsg;
        public bool ShowErrorMsg
        {
            get => _showErrorMsg;
            set => this.RaiseAndSetIfChanged(ref _showErrorMsg, value);
        }

        public ThreadsListUserControlViewModel() { }

        public ThreadsListUserControlViewModel(IFlashbackService flashbackService, string forumUrl)
        {
            _flashbackService = flashbackService;
            ForumUrl = forumUrl;
        }

        public async Task InitializeAsync()
        {
            try
            {
                ShowErrorMsg = false;
                IsLoading = true;
                ShowRetryButton = false;

                LoadingText = $"Hämtar data...";

                ThreadListPage = await _flashbackService.GetThreadListPageAsync(ForumUrl);

                IsLoading = false;
            }
            catch
            {
                IsLoading = false;
                ShowRetryButton = true;
                ShowErrorMsg = true;
            }
        }
    }
}