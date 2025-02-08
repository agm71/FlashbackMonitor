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
using System.Windows.Input;

namespace FlashbackMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public bool ApplySettings { get; set; } = true;
        public ObservableCollection<NotificationViewModel> NotificationItems { get; } = [];
        public ObservableCollection<ForumViewModel> ForumItems { get; } = [];
        public ObservableCollection<UserViewModel> Users { get; set; } = [];
        public ObservableCollection<TopicViewModel> Topics { get; set; } = [];

        private string _updatedText;
        public string UpdatedText
        {
            get => _updatedText;
            set => this.RaiseAndSetIfChanged(ref _updatedText, value);
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

        public int Interval { get; set; }

        public void RaiseNotificationForAllChecked()
        {
            this.RaisePropertyChanged(nameof(AllChecked));
        }

        public ICommand SaveSettingsCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand RemoveUserCommand { get; }

        private readonly Timer _timer;
        private readonly IFlashbackService _flashbackService;
        private readonly ISettingsService _settingsService;
        private IEnumerable<FlashbackDataItem> _fbItems = [];

        public MainWindowViewModel() { }

        public MainWindowViewModel(IFlashbackService flashbackService, ISettingsService settingsService)
        {
            SaveSettingsCommand = ReactiveCommand.CreateFromTask(SaveSettingsAsync);
            AddUserCommand = ReactiveCommand.Create<string>((userName) => AddUser(userName));
            RemoveUserCommand = ReactiveCommand.Create<string>((userName) => RemoveUser(userName));

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

        private void AddUser(string userName)
        {
            Users.Add(new()
            {
                UserName = userName,
            });
        }

        private void RemoveUser(string userName)
        {
            Users.RemoveMany(Users.Where(x => x.UserName == userName).ToList());
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await LoadDataAsync(false);
        }

        public static async Task<MainWindowViewModel> CreateAsync(IFlashbackService flashbackService, ISettingsService settingsService)
        {
            var viewModel = new MainWindowViewModel(flashbackService, settingsService);
            
            await viewModel.LoadDataAsync(true);
            
            return viewModel;
        }

        public async Task LoadDataAsync(bool firstExecution)
        {
            var data = await _flashbackService.GetFlashbackDataAsync();
            var settings = await _settingsService.GetSettingsAsync();

            if (firstExecution)
            {
                ForumItems.AddRange(data.Select(x => new ForumViewModel(this) { Name = x.ForumName, IsChecked = false, ForumColor = x.ForumColor }));
            }

            data = data.OrderByDescending(x => x.TopicLastUpdatedDateTime)
                .Where(d => settings.Forums.Contains(d.ForumName) || settings.Topics.Exists(x => string.Equals(x.TopicName, d.TopicName, StringComparison.OrdinalIgnoreCase)));

            var newItems = data
                .Join(_fbItems,
                        item1 => item1.Index,
                        item2 => item2.Index,
                        (item1, item2) => new { item1, item2 })
                .Where(x => x.item1.TopicLastUpdated != x.item2.TopicLastUpdated || x.item1.TopicName != x.item2.TopicName || x.item1.UserName != x.item2.UserName)
                    .OrderBy(x => x.item1.TopicLastUpdatedDateTime)
                    .Select(x => x.item1);

            if (settings.Users.Any(u => u.VipUser))
            {
                newItems = newItems.Where(d => settings.Users.Any(u => string.Equals(u.UserName, d.UserName, StringComparison.OrdinalIgnoreCase) && u.VipUser));
            }
            else if (settings.Users.Any(u => u.IgnoredUser))
            {
                newItems = newItems.Where(d => settings.Users.Any(u => !string.Equals(u.UserName, d.UserName, StringComparison.OrdinalIgnoreCase) && u.IgnoredUser));
            }

            if (newItems.Any())
            {
                foreach (var item in newItems)
                {
                    NotificationItems.Insert(0, new NotificationViewModel
                    {
                        UserName = item.UserName,
                        VipUser = settings.Users.Any(u => string.Equals(u.UserName, item.UserName, StringComparison.OrdinalIgnoreCase) && u.VipUser),
                        IsFavoriteUser = settings.Users.Any(u => string.Equals(u.UserName, item.UserName, StringComparison.OrdinalIgnoreCase) && u.Favorite),
                        TopicName = item.TopicName,
                        TopicUrl = item.TopicUrl,
                        TopicLastUpdated = item.TopicLastUpdated,
                        TopicColor = item.ForumColor,
                        IsFavoriteTopic = settings.Topics.Exists(x => string.Equals(x.TopicName, item.TopicName, StringComparison.OrdinalIgnoreCase) && x.IsFavoriteTopic)
                    });
                }
            }
            else if (firstExecution)
            {
                foreach (var item in (settings.Users.Any(u => u.VipUser) 
                    ? data.Where(d => settings.Users.Any(u => string.Equals(u.UserName, d.UserName, StringComparison.OrdinalIgnoreCase) && u.VipUser))
                    : data.Where(d => !settings.Users.Any(u => string.Equals(u.UserName, d.UserName, StringComparison.OrdinalIgnoreCase) && u.IgnoredUser))))
                {
                    NotificationItems.Add(new NotificationViewModel
                    {
                        UserName = item.UserName,
                        VipUser = settings.Users.Any(u => string.Equals(u.UserName, item.UserName, StringComparison.OrdinalIgnoreCase) && u.VipUser),
                        IsFavoriteUser = settings.Users.Any(u => string.Equals(u.UserName, item.UserName, StringComparison.OrdinalIgnoreCase) && u.Favorite),
                        TopicName = item.TopicName,
                        TopicUrl = item.TopicUrl,
                        TopicLastUpdated = item.TopicLastUpdated,
                        TopicColor = item.ForumColor,
                        IsFavoriteTopic = settings.Topics.Exists(x => string.Equals(x.TopicName, item.TopicName, StringComparison.OrdinalIgnoreCase) && x.IsFavoriteTopic)
                    });
                }

                Topics = new ObservableCollection<TopicViewModel>(settings.Topics);
                Users = new ObservableCollection<UserViewModel>(settings.Users);
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

                Interval = settings.Interval;
                _timer.Interval = settings.Interval * 1000;
                _timer.Enabled = true;
                ApplySettings = false;
            }

            _fbItems = data;

            UpdatedText = $"Uppdaterad: {DateTime.Now}";
        }
    }
}