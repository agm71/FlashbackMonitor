using Avalonia.Controls;
using FlashbackMonitor.ViewModels;
using FlashbackMonitor.Services;
using System;
using System.Diagnostics;

namespace FlashbackMonitor.Views
{
    public partial class MainWindow : Window
    {
        private MainUserControl _mainUserControl = new MainUserControl();
        private TopicUserControl _topicUserControl = new TopicUserControl();
        private ThreadListUserControl _threadListUserControl = new ThreadListUserControl();

        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = new MainWindowViewModel(new FlashbackService(), new SettingsService());

            ShowMainUserControl();

            _mainUserControl.NavigateToSettings += ShowSettingsUserControl;

            ContentHost1.Content = _mainUserControl;
        }

        private void ShowMainUserControl()
        {
            ContentHost1.Content = null;
            _mainUserControl.NavigateToTopic += ShowTopicUserControl;
            _mainUserControl.NavigateToThreadList += ShowThreadListUserControl;
            ContentHost1.Content = _mainUserControl;
        }

        private void ShowThreadListUserControl(string forumUrl = null)
        {
            ContentHost1.Content = null;
            _threadListUserControl = new ThreadListUserControl(forumUrl);
            _threadListUserControl.NavigateToMain += ShowMainUserControl;
            _threadListUserControl.NavigateToTopic += ShowTopicUserControl;
            ContentHost1.Content = _threadListUserControl;
        }

        private void ShowTopicUserControl(string t, string from = null)
        {
            ContentHost1.Content = null;
            _topicUserControl = new TopicUserControl(t, from);
            _topicUserControl.NavigateToMain += ShowMainUserControl;
            _topicUserControl.NavigateToThreadList += ShowThreadListUserControl;
            ContentHost1.Content = _topicUserControl;
        }

        private void ShowSettingsUserControl()
        {
            var settingsUserControl = new SettingsUserControl();

            settingsUserControl.NavigateToMain += ShowMainUserControl;
            ContentHost1.Content = settingsUserControl;
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.LoadDataAsync(true);
            }
        }

        private void MenuItemFlashback_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var topicUrl = menuItem.Tag as string;

            Process.Start(new ProcessStartInfo
            {
                FileName = topicUrl,
                UseShellExecute = true
            });
        }

        private void MenuItemAddFavoriteTopic_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            var topicName = (sender as MenuItem).Tag as string;
            viewModel.Topics.Insert(0, new TopicViewModel()
            {
                TopicName = topicName,
                IsFavoriteTopic = true,
            });
            viewModel.SaveSettingsCommand.Execute(null);
        }
    }
}