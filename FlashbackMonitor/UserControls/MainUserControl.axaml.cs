using Avalonia.Controls;
using FlashbackMonitor.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace FlashbackMonitor;

public partial class MainUserControl : UserControl
{
    public event Action<string> NavigateToTopic;
    public event Action NavigateToSettings;

    public MainUserControl()
    {
        InitializeComponent();
    }

    private void TextBlock_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (sender is TextBlock clickedTextBlock)
            {
                var tagValue = clickedTextBlock.Tag?.ToString();
                NavigateToTopic?.Invoke(tagValue);
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

    private void MenuItemRemoveTopic_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var topicName = (sender as MenuItem).Tag as string;
        var topic = viewModel.Topics.FirstOrDefault(t => string.Equals(t.TopicName, topicName, StringComparison.OrdinalIgnoreCase));
        
        if (topic != null)
        {
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
            viewModel.Users.Remove(user);
        }
       
        viewModel.SaveSettingsCommand.Execute(null);
    }
}