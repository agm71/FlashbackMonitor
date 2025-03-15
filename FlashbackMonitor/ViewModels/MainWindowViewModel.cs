using DynamicData;
using FlashbackMonitor.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

namespace FlashbackMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<NotificationViewModel> VisibleNotificationItems { get; set; } = [];
        public ObservableCollection<NotificationViewModel> AllNotificationItems { get; } = [];
        public ObservableCollection<ForumViewModel> ForumItems { get; } = [];
        public ObservableCollection<UserViewModel> Users { get; set; } = [];
        public ObservableCollection<TopicViewModel> Topics { get; set; } = [];
        public ObservableCollection<CategoryWithForumsViewModel> CategoryWithForums { get; set; } = [];

        private static CancellationTokenSource cancellationTokenSource;
        CancellationToken token;

        private bool _showOverviewView;
        public bool ShowOverviewView
        {
            get => _showOverviewView;
            set => this.RaiseAndSetIfChanged(ref _showOverviewView, value);
        }

        private bool _showNotificationView = true;
        public bool ShowNotificationView
        {
            get => _showNotificationView;
            set => this.RaiseAndSetIfChanged(ref _showNotificationView, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                this.RaiseAndSetIfChanged(ref _searchText, value);
                ApplyFilter();
            }        
        }

        private int _selectedTopicIndex2 = 0;
        public int SelectedTopicIndex2
        {
            get => _selectedTopicIndex2;
            set
            {
                _selectedTopicIndex2 = value;
            }
        }

        private int _selectedTopicIndex = -1;
        public int SelectedTopicIndex
        {
            get => _selectedTopicIndex;
            set
            {
                _selectedTopicIndex = value;              
                ApplyFilter();
            }
        }

        private int _selectedUserIndex = -1;
        public int SelectedUserIndex
        {
            get => _selectedUserIndex;
            set
            {
                _selectedUserIndex = value;
                ApplyFilter();
            }
        }

        public void ApplyFilter()
        {           
            var notificationsToShow = Enumerable.Empty<NotificationViewModel>();

            if (SelectedTopicIndex == 0)
            {
                notificationsToShow = AllNotificationItems;
            }
            else if (SelectedTopicIndex == 1) // Prenumererade trådar
            {
                var filteredItems = AllNotificationItems.Where(x => _settings.Topics.Any(t => string.Equals(t.TopicName, x.TopicName, StringComparison.OrdinalIgnoreCase)));

                notificationsToShow = filteredItems;
            }
            else if (SelectedTopicIndex == 2) // Prenumererade trådar (favoriter)
            {
                var favoriteTopicNotifications = AllNotificationItems.Where(x => _settings.Topics.Any(t => t.IsFavoriteTopic && string.Equals(t.TopicName, x.TopicName, StringComparison.OrdinalIgnoreCase)));
                notificationsToShow = favoriteTopicNotifications;
            }
            else if (SelectedTopicIndex == 3) // Prenumererade forum
            {
                var favoriteTopicNotifications = AllNotificationItems.Where(x => _settings.Forums.Any(f => string.Equals(f, x.ForumName, StringComparison.OrdinalIgnoreCase)));
                notificationsToShow = favoriteTopicNotifications;
            }
            else if (SelectedTopicIndex == 4) // Prenumererade forum + prenumererade trådar
            {
                var favoriteTopicNotifications = AllNotificationItems.Where(x => _settings.Forums.Any(f => string.Equals(f, x.ForumName, StringComparison.OrdinalIgnoreCase))
                    || _settings.Topics.Any(t => string.Equals(t.TopicName, x.TopicName, StringComparison.OrdinalIgnoreCase)));

                notificationsToShow = favoriteTopicNotifications;
            }

            if (SelectedUserIndex == 1) // Favoritanvändare
            {
                notificationsToShow = notificationsToShow.Where(x => _settings.Users.Any(u => string.Equals(u.UserName, x.UserName, StringComparison.OrdinalIgnoreCase) && u.Favorite));
            }

            if (_settings.Users.Any(u => u.VipUser))
            {
                notificationsToShow = notificationsToShow.Where(d => _settings.Users.Any(u => string.Equals(u.UserName, d.UserName, StringComparison.OrdinalIgnoreCase) && u.VipUser));
            }
            else if (_settings.Users.Any(u => u.IgnoredUser))
            {
                notificationsToShow = notificationsToShow.Where(d => _settings.Users.Any(u => !string.Equals(u.UserName, d.UserName, StringComparison.OrdinalIgnoreCase) && u.IgnoredUser));
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                notificationsToShow = notificationsToShow.Where(x => x.TopicName.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) || x.UserName.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()));
            }

            VisibleNotificationItems.Clear();
            VisibleNotificationItems.AddRange(notificationsToShow);
        }

        private string _updateStatus;
        public string UpdateStatus
        {
            get => _updateStatus;
            set => this.RaiseAndSetIfChanged(ref _updateStatus, value);
        }

        private string _updateStatusDescription;
        public string UpdateStatusDescription
        {
            get => _updateStatusDescription;
            set => this.RaiseAndSetIfChanged(ref _updateStatusDescription, value);
        }

        public bool AllChecked
        {
            get => ForumItems.All(x => x.IsChecked);
            set
            {
                foreach (var item in ForumItems)
                {
                    item.IsChecked = value;
                }
            }
        }

        public void RaiseNotificationForAllChecked()
        {
            this.RaisePropertyChanged(nameof(AllChecked));
        }

        public void RaiseNotificationForAllItems(string topicName, bool c)
        {
            var x = AllNotificationItems.FirstOrDefault(x => string.Equals(x.TopicName, topicName, StringComparison.OrdinalIgnoreCase));
            if (x != null)
            {
                x.IsFavoriteTopic = c;
            }
        }

        private int _interval;
        public int Interval
        {
            get => _interval;
            set => this.RaiseAndSetIfChanged(ref _interval, value); 
        }

        public ICommand SaveSettingsCommand { get; }

        private readonly IFlashbackService _flashbackService;
        private readonly ISettingsService _settingsService;

        private IEnumerable<FlashbackDataItem> _fbItems = [];
        public Settings _settings;

        public int MessageLoadingCount { get; set; } = 1;

        private bool _messageLoadingCountGreaterThanOne;
        public bool MessageLoadingCountGreaterThanOne
        {
            get => _messageLoadingCountGreaterThanOne;
            set => this.RaiseAndSetIfChanged(ref _messageLoadingCountGreaterThanOne, value);
        }

        private bool _isError;
        public bool IsError
        {
            get => _isError;
            set => this.RaiseAndSetIfChanged(ref _isError, value);
        }

        private string _loadingText;
        public string LoadingText
        {
            get => _loadingText;
            set => this.RaiseAndSetIfChanged(ref _loadingText, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public MainWindowViewModel() { }

        public MainWindowViewModel(IFlashbackService flashbackService, ISettingsService settingsService)
        {
            SaveSettingsCommand = ReactiveCommand.CreateFromTask(SaveSettingsAsync);

            _flashbackService = flashbackService;
            _settingsService = settingsService;
            _settings = _settingsService.GetSettings();

            Topics.AddRange(_settings.Topics);
            Users.AddRange(_settings.Users);
            Interval = _settings.Interval;

            if (_settings.Topics.Count != 0 && _settings.Topics.Count != 0)
            {
                SelectedTopicIndex = 4;
            }
            else if (_settings.Topics.Count != 0)
            {
                SelectedTopicIndex = 1;
            }
            else if (_settings.Forums.Count != 0)
            {
                SelectedTopicIndex = 3;
            }
            else
            {
                SelectedTopicIndex = 0;
            }

            SelectedUserIndex = 0;
        }

        private async Task SaveSettingsAsync()
        {
            await _settingsService.SaveSettingsAsync(this);
            _settings = await _settingsService.GetSettingsAsync();
            Interval = _settings.Interval;
            cancellationTokenSource.Cancel();
            ApplyFilter();
        }

        public async Task LoadDataAsync(bool initial)
        {
            

        LoadDataAsync:
            MessageLoadingCount = 1;
            IsLoading = true;
            UpdateStatus = HttpUtility.HtmlDecode("&#128993;");
            UpdateStatusDescription = "Hämtar data";

            IEnumerable<FlashbackDataItem> data;

            cancellationTokenSource = new CancellationTokenSource();
            token = cancellationTokenSource.Token;

        getData:
            try
            {
                data = await _flashbackService.GetFlashbackDataAsync();
            }
            catch
            {
                IsError = true;
                UpdateStatus = HttpUtility.HtmlDecode("&#128992;");
                UpdateStatusDescription = $"Fel vid datahämtning. Gör försök nummer {++MessageLoadingCount}";

                await Task.Delay(5000);
               
                goto getData;
            }

            _settings = await _settingsService.GetSettingsAsync();

            if (initial)
            {
                ForumItems.AddRange(data.Select(x => new ForumViewModel(this)
                {
                    Name = x.ForumName,
                    IsChecked = _settings.Forums.Exists(s => s == x.ForumName),
                    ForumColor = x.ForumColor
                }));
            }

            data = data.OrderByDescending(x => x.TopicLastUpdatedDateTime);

            List<NotificationViewModel> notItems = new List<NotificationViewModel>();
            foreach (var item in data)
            {
                notItems.Add(new NotificationViewModel
                {
                    UserName = item.UserName,
                    VipUser = _settings.Users.Any(u => string.Equals(u.UserName, item.UserName, StringComparison.OrdinalIgnoreCase) && u.VipUser),
                    IsFavoriteUser = _settings.Users.Any(u => string.Equals(u.UserName, item.UserName, StringComparison.OrdinalIgnoreCase) && u.Favorite),
                    TopicName = item.TopicName,
                    TopicUrl = item.TopicUrl,
                    TopicLastUpdated = item.TopicLastUpdated,
                    TopicColor = item.ForumColor,
                    IsFavoriteTopic = _settings.Topics.Exists(x => string.Equals(x.TopicName, item.TopicName, StringComparison.OrdinalIgnoreCase) && x.IsFavoriteTopic),
                    ForumName = item.ForumName,
                    Index = item.Index,
                    ForumCategory = item.ForumCategory,
                    ForumUrl = item.ForumUrl
                });
            }

            CategoryWithForums.Clear();
            CategoryWithForums.AddRange(notItems.OrderBy(x => x.Index).GroupBy(x => x.ForumCategory).Select(x => new CategoryWithForumsViewModel
            {
                Category = x.Key,
                ForumColor = x.First().TopicColor,
                Notifications = [.. x]
            }));

            if (initial)
            {
                AllNotificationItems.AddRange(notItems);
            }
            else
            {
                var newItems = data
                    .Join(_fbItems,
                            item1 => item1.Index,
                            item2 => item2.Index,
                            (item1, item2) => new { item1, item2 })
                    .Where(x => x.item1.TopicLastUpdated != x.item2.TopicLastUpdated || x.item1.TopicName != x.item2.TopicName || x.item1.UserName != x.item2.UserName)
                        .OrderByDescending(x => x.item1.TopicLastUpdatedDateTime)
                        .Select(x => x.item1);

                if (newItems.Any())
                {
                    foreach (var item in newItems.OrderBy(x => x.TopicLastUpdatedDateTime))
                    {
                        AllNotificationItems.Insert(0, new NotificationViewModel
                        {
                            UserName = item.UserName,
                            VipUser = _settings.Users.Any(u => string.Equals(u.UserName, item.UserName, StringComparison.OrdinalIgnoreCase) && u.VipUser),
                            IsFavoriteUser = _settings.Users.Any(u => string.Equals(u.UserName, item.UserName, StringComparison.OrdinalIgnoreCase) && u.Favorite),
                            TopicName = item.TopicName,
                            TopicUrl = item.TopicUrl,
                            TopicLastUpdated = item.TopicLastUpdated,
                            TopicColor = item.ForumColor,
                            IsFavoriteTopic = _settings.Topics.Exists(x => string.Equals(x.TopicName, item.TopicName, StringComparison.OrdinalIgnoreCase) && x.IsFavoriteTopic),
                            ForumName = item.ForumName,
                            Index = item.Index,
                            ForumCategory = item.ForumCategory,
                            ForumUrl = item.ForumUrl
                        });
                    }
                }
            }

            _fbItems = data;

            ApplyFilter();

            UpdateStatus = HttpUtility.HtmlDecode("&#128994;");
            UpdateStatusDescription = $"Uppdaterad: {DateTime.Now}";

            IsLoading = false;

            IsError = false;

            initial = false;

            try
            {
                await Task.Delay(Interval * 1000, token);
            }
            catch (TaskCanceledException)
            {
                goto LoadDataAsync;
            }
            
            goto LoadDataAsync;
        }
    }
}