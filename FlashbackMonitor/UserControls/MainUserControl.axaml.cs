using Avalonia.Controls;
using FlashbackMonitor.Utils;
using FlashbackMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FlashbackMonitor;

public partial class MainUserControl : UserControl
{
    public event Action<Stack<NavigationInfo>> NavigateToTopic;

    public event Action NavigateToSettings;
    public event Action<Stack<NavigationInfo>> NavigateToThreadList;
    public Stack<NavigationInfo> NavigationInfoStack {  get; set; } = new Stack<NavigationInfo>();

    public MainUserControl(Stack<NavigationInfo> navigationInfo)
    {
        InitializeComponent();
        NavigationInfoStack = navigationInfo ?? new Stack<NavigationInfo>();
    }

    public MainUserControl() { }

    private void TextBlock_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (sender is TextBlock clickedTextBlock)
            {
                var tagValue = clickedTextBlock.Tag?.ToString();

                NavigationInfoStack.Push(
                new NavigationInfo
                {
                    PreviousUrl = null,
                    PreviousUserControl = UserControlType.MainUserControl,
                    RequestedUrl = tagValue,
                    RequestedUserControl = UserControlType.TopicUserControl
                });

                NavigateToTopic?.Invoke(NavigationInfoStack);
            }
        }
    }

    private void SettingsImage_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is Image clickedImage)
        {
            NavigateToSettings?.Invoke();
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
        var viewModel = DataContext as MainWindowViewModel;
        var topicName = (sender as MenuItem).Tag as string;
        var topic = viewModel.Topics.FirstOrDefault(t => string.Equals(t.TopicName, topicName, StringComparison.OrdinalIgnoreCase));

        if (topic == null)
        {
            viewModel.Topics.Insert(0, new TopicViewModel()
            {
                TopicName = topicName
            });
        }

        viewModel.SaveSettingsCommand.Execute(null);
    }

    private void MenuItemAddFavoriteTopic_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var topicName = (sender as MenuItem).Tag as string;
        var topic = viewModel.Topics.FirstOrDefault(t => string.Equals(t.TopicName, topicName, StringComparison.OrdinalIgnoreCase));

        if (topic == null)
        {
            viewModel.Topics.Insert(0, new TopicViewModel()
            {
                TopicName = topicName,
                IsFavoriteTopic = true
            });
        }
        else
        {
            topic.IsFavoriteTopic = true;
        }

        viewModel.SaveSettingsCommand.Execute(null);
    }

    private void MenuItemRemoveTopic_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var topicName = (sender as MenuItem).Tag as string;
        var topic = viewModel.Topics.FirstOrDefault(t => string.Equals(t.TopicName, topicName, StringComparison.OrdinalIgnoreCase));

        if (topic != null)
        {
            foreach (var t in viewModel.AllNotificationItems.Where(x => x.TopicName == topicName))
            {
                t.IsFavoriteTopic = false;
            }

            viewModel.Topics.Remove(topic);
        }

        viewModel.SaveSettingsCommand.Execute(null);
    }

    private void MenuItemAddUser_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var clickedUserName = (sender as MenuItem).Tag as string;
        var user = viewModel.Users.FirstOrDefault(x => string.Equals(x.UserName, clickedUserName, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            viewModel.Users.Insert(0, new UserViewModel()
            {
                UserName = clickedUserName
            });
        }

        viewModel.SaveSettingsCommand.Execute(null);
    }

    private void MenuItemRemoveUser_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var clickedUserName = (sender as MenuItem).Tag as string;
        var user = viewModel.Users.FirstOrDefault(x => string.Equals(x.UserName, clickedUserName, StringComparison.OrdinalIgnoreCase));

        if (user != null)
        {
            foreach (var u in viewModel.AllNotificationItems.Where(x => x.UserName == user.UserName))
            {
                u.IsFavoriteUser = false;
            }

            viewModel.Users.Remove(user);
        }

        viewModel.SaveSettingsCommand.Execute(null);
    }

    private void ViewComboBox_SelectionChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var comboBox = sender as ComboBox;
        var selectedIndex = comboBox.SelectedIndex;

        var scrollViewer = this.FindControl<ScrollViewer>("SV");

        var searchTextBox = this.FindControl<TextBox>("SearchTextBox");
        var notificationsFilterComboBox = this.FindControl<ComboBox>("NotificationsFilterComboBox");
        var usersFilterComboBox = this.FindControl<ComboBox>("UsersFilterComboBox");

        if (selectedIndex == 0)
        {
            viewModel.ShowOverviewView = false;
            viewModel.ShowNotificationView = true;

            searchTextBox.IsEnabled = true;
            notificationsFilterComboBox.IsEnabled = true;
            usersFilterComboBox.IsEnabled = true;
        }
        else if (selectedIndex == 1)
        {
            viewModel.ShowNotificationView = false;
            viewModel.ShowOverviewView = true;

            searchTextBox.IsEnabled = false;
            notificationsFilterComboBox.IsEnabled = false;
            usersFilterComboBox.IsEnabled = false;
        }

        scrollViewer.ScrollToHome();
    }

    private void ForumNameTextBlock_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (sender is TextBlock clickedTextBlock)
            {
                var forumUrl = clickedTextBlock.Tag?.ToString();

                NavigationInfoStack.Push(
                    new NavigationInfo
                    {
                        PreviousUrl = null,
                        PreviousUserControl = UserControlType.MainUserControl,
                        RequestedUrl = forumUrl,
                        RequestedUserControl = UserControlType.ThreadListUserControl
                    });

                NavigateToThreadList?.Invoke(NavigationInfoStack);
            }
        }
    }
}