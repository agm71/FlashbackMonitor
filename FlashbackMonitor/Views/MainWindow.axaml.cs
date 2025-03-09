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

        public MainWindowViewModel ViewModel { get; set; }

        private MainUserControl mainUC = new();

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
            ContentHost1.Content = _mainUserControl;
        }

        private void ShowTopicUserControl(string t)
        {
            ContentHost1.Content = null;
            _topicUserControl = new TopicUserControl(t);
            _topicUserControl.NavigateToMain += ShowMainUserControl;
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