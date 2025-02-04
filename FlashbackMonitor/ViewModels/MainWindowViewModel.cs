using DynamicData;
using FlashbackMonitor.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Input;

namespace FlashbackMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }
        public string FlashbackMonitorText { get; set; } = "Fl";

        public bool ApplySettings { get; set; } = true;
        public ObservableCollection<NotificationViewModel> NotificationItems { get; } = [];
        public ObservableCollection<ForumViewModel> ForumItems { get; } = [];

        private string _topicSubscriptions;
        public string TopicSubscriptions
        {
            get => _topicSubscriptions;
            set => this.RaiseAndSetIfChanged(ref _topicSubscriptions, value);
        }

        private string _favoriteTopics;
        public string FavoriteTopics
        {
            get => _favoriteTopics;
            set => this.RaiseAndSetIfChanged(ref _favoriteTopics, value);
        }

        public ICommand SaveSettingsCommand { get; }

        public bool AllChecked
        {
            get => ForumItems.All(x => x.IsChecked);
            set {
                foreach (var item in ForumItems)
                {
                    item.IsChecked = value;
                }
            }
        }

        private List<FlashbackDataItem> _fbItems = [];

        private readonly Timer _timer;

        public int Interval { get; set; }

        private readonly IFlashbackService _flashbackService;
        private readonly ISettingsService _settingsService;

        public MainWindowViewModel(IFlashbackService flashbackService, ISettingsService settingsService)
        {
            SaveSettingsCommand = ReactiveCommand.CreateFromTask(SaveSettingsAsync);

            _flashbackService = flashbackService;
            _settingsService = settingsService;

            _timer = new Timer();
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = false;
        }

        private async Task SaveSettingsAsync()
        {
            await _settingsService.SaveSettingsAsync(this);
            ApplySettings = true;
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await LoadDataAsync(false);
        }

        public MainWindowViewModel() { }

        public static async Task<MainWindowViewModel> CreateAsync(IFlashbackService flashbackService, ISettingsService settingsService)
        {
            var viewModel = new MainWindowViewModel(flashbackService, settingsService);
            await viewModel.LoadDataAsync(true);
            return viewModel;
        }

        public async Task LoadDataAsync(bool firstExecution)
        {
            IsLoading = true;

            var data = await _flashbackService.GetFlashbackDataAsync();
            var settings = await _settingsService.GetSettingsAsync();

            if (firstExecution)
            {
                List<string> _forumItems = [.. data.Select(x => x.ForumName)];
                ForumItems.AddRange(_forumItems.Select(x => new ForumViewModel { Name = x, IsChecked = false }));
            }

            data = [.. data.OrderByDescending(x => x.TopicLastUpdatedDateTime)];

            data = data.Where(d => settings.Forums.Contains(d.ForumName) || settings.Topics.Exists(x => string.Equals(x, d.TopicName, StringComparison.OrdinalIgnoreCase))).ToList();

            var newItems = data
                .Join(_fbItems,
                        item1 => item1.Index,
                        item2 => item2.Index,
                        (item1, item2) => new { item1, item2 })
                .Where(x => x.item1.TopicLastUpdated != x.item2.TopicLastUpdated || x.item1.TopicName != x.item2.TopicName || x.item1.UserName != x.item2.UserName).Select(x => x.item1)
                .ToList();

            if (newItems.Count > 0)
            {
                foreach (var item in newItems)
                {
                    NotificationItems.Insert(0, new NotificationViewModel
                    {
                        UserName = item.UserName,
                        TopicName = item.TopicName,
                        TopicUrl = item.TopicUrl,
                        TopicLastUpdated = item.TopicLastUpdated,
                        TopicColor = item.ForumColor,
                        FavoriteTopic = settings.FavoriteTopics.Exists(x => string.Equals(x, item.TopicName, StringComparison.OrdinalIgnoreCase)) ? HttpUtility.HtmlDecode("&#9733;") : ""
                    });
                }
            }
            else if (firstExecution)
            {
                foreach (var item in data)
                {
                    NotificationItems.Add(new NotificationViewModel
                    {
                        UserName = item.UserName,
                        TopicName = item.TopicName,
                        TopicUrl = item.TopicUrl,
                        TopicLastUpdated = item.TopicLastUpdated,
                        TopicColor = item.ForumColor,
                        FavoriteTopic = settings.FavoriteTopics.Exists(x => string.Equals(x, item.TopicName, StringComparison.OrdinalIgnoreCase)) ? HttpUtility.HtmlDecode("&#9733;") : ""
                    });
                }
            }
            
            if (ApplySettings)
            {
                foreach (var forumName in settings.Forums)
                {
                    var forum = ForumItems.FirstOrDefault(x => x.Name == forumName);

                    if (forum != null)
                    {
                        forum.IsChecked = true;
                    }
                }

                TopicSubscriptions = string.Join(Environment.NewLine, settings.Topics);
                FavoriteTopics = string.Join(Environment.NewLine, settings.FavoriteTopics);

                Interval = settings.Interval;

                ApplySettings = false;
            }

            _fbItems = data;
            _timer.Interval = settings.Interval * 1000;
            _timer.Enabled = true;

            IsLoading = false;
        }
    }
}