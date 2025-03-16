using FlashbackMonitor.Services;
using ReactiveUI;
using System.Threading.Tasks;

namespace FlashbackMonitor.ViewModels
{
    public class TopicUserControlViewModel : ViewModelBase
    {
        private readonly IFlashbackService _flashbackService;

        public string _topicUrl;
        public string TopicUrl
        {
            get => _topicUrl;
            set => this.RaiseAndSetIfChanged(ref _topicUrl, value);
        }

        public bool FirstRun { get; set; } = true;

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public int MessageLoadingCount { get; set; } = 1;

        private string _loadingText;
        public string LoadingText
        {
            get => _loadingText;
            set => this.RaiseAndSetIfChanged(ref _loadingText, value);
        }

        private TopicPage _topicPage;
        public TopicPage TopicPage
        {
            get => _topicPage;
            set => this.RaiseAndSetIfChanged(ref _topicPage, value);
        }

        public TopicUserControlViewModel(IFlashbackService flashbackService, string topicUrl)
        {
            _flashbackService = flashbackService;
            TopicUrl = topicUrl;
        }

        private int _selectedPageIndex = -1;
        public int SelectedPageIndex
        {
            get => _selectedPageIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedPageIndex, value);
            }
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

        public TopicUserControlViewModel() { }

        public async Task InitializeAsync()
        {
            try
            {
                ShowErrorMsg = false;

                IsLoading = true;
                ShowRetryButton = false;

                LoadingText = $"Hämtar data...";

                TopicPage = await _flashbackService.GetTopicPageAsync(TopicUrl);
                
                SelectedPageIndex = TopicPage.CurrentPage -1;
                FirstRun = false;
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