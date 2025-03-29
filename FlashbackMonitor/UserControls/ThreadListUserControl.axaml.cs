using Avalonia.Controls;
using FlashbackMonitor.Services;
using FlashbackMonitor.Utils;
using FlashbackMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FlashbackMonitor;

public partial class ThreadListUserControl : UserControl
{
    public event Action NavigateToMain;

    public event Action<Stack<NavigationInfo>> NavigateToTopic;

    public event Action<Stack<NavigationInfo>> NavigateToSubThreadList;

    public event Action<Stack<NavigationInfo>> NavigateToPreviousThreadList;

    public ThreadsListUserControlViewModel ViewModel { get; }
    public string ForumUrl { get; set; }

    public Stack<NavigationInfo> Stack { get; set; }

    public MainWindowViewModel MainWindowViewModel { get; set; }

    public ThreadListUserControl(Stack<NavigationInfo> navigationInfo, MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        ViewModel = new ThreadsListUserControlViewModel(new FlashbackService(), navigationInfo.First().RequestedUrl);
        DataContext = ViewModel;
        Stack = navigationInfo;
        MainWindowViewModel = mainWindowViewModel;
    }

    public ThreadListUserControl()
    {
        InitializeComponent();
        ViewModel = new ThreadsListUserControlViewModel(new FlashbackService(), ForumUrl);
        DataContext = ViewModel;
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();

        if (DataContext is ThreadsListUserControlViewModel viewModel)
        {
            await ViewModel.InitializeAsync();
        }
    }

    private void BackImage_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Stack.Pop();
        if (Stack.Count == 0 || Stack.First().RequestedUserControl == UserControlType.MainUserControl)
        {
            NavigateToMain?.Invoke();
        }
        else
        {
            NavigateToPreviousThreadList?.Invoke(Stack);
        }
    }

    private void TopicNameTextBlock_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (sender is TextBlock clickedTextBlock)
            {
                var topicUrl = clickedTextBlock.Tag?.ToString();

                Stack.Push(new NavigationInfo
                {
                    PreviousUrl = null,
                    PreviousUserControl = UserControlType.ThreadListUserControl,
                    RequestedUrl = topicUrl,
                    RequestedUserControl = UserControlType.TopicUserControl,
                });

                NavigateToTopic?.Invoke(Stack);
            }
        }
    }

    private async void RetryGettingThreadsButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private void ForumNameTextBlock_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (sender is TextBlock clickedTextBlock)
            {
                var forumUrl = clickedTextBlock.Tag?.ToString();

                Stack.Push(new NavigationInfo
                {
                    PreviousUrl = null,
                    PreviousUserControl = UserControlType.ThreadListUserControl,
                    RequestedUrl = forumUrl,
                    RequestedUserControl = UserControlType.ThreadListUserControl,
                });

                NavigateToSubThreadList?.Invoke(Stack);
            }
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

    private void MenuItemAddTopic_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topicName = (sender as MenuItem).Tag as string;
        var topic = MainWindowViewModel.Topics.FirstOrDefault(t => string.Equals(t.TopicName, topicName, StringComparison.OrdinalIgnoreCase));

        if (topic == null)
        {
            MainWindowViewModel.Topics.Insert(0, new TopicViewModel()
            {
                TopicName = topicName
            });
        }

        MainWindowViewModel.SaveSettingsCommand.Execute(null);
    }

    private void MenuItemAddFavoriteTopic_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topicName = (sender as MenuItem).Tag as string;
        var topic = MainWindowViewModel.Topics.FirstOrDefault(t => string.Equals(t.TopicName, topicName, StringComparison.OrdinalIgnoreCase));

        if (topic == null)
        {
            MainWindowViewModel.Topics.Insert(0, new TopicViewModel()
            {
                TopicName = topicName,
                IsFavoriteTopic = true
            });
        }
        else
        {
            topic.IsFavoriteTopic = true;
        }

        MainWindowViewModel.SaveSettingsCommand.Execute(null);
    }

    private void MenuItemRemoveTopic_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topicName = (sender as MenuItem).Tag as string;
        var topic = MainWindowViewModel.Topics.FirstOrDefault(t => string.Equals(t.TopicName, topicName, StringComparison.OrdinalIgnoreCase));

        if (topic != null)
        {
            foreach (var t in MainWindowViewModel.AllNotificationItems.Where(x => x.TopicName == topicName))
            {
                t.IsFavoriteTopic = false;
            }

            MainWindowViewModel.Topics.Remove(topic);
        }

        MainWindowViewModel.SaveSettingsCommand.Execute(null);
    }
}