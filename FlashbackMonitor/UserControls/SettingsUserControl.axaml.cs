using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using FlashbackMonitor.ViewModels;
using System;
using System.Linq;

namespace FlashbackMonitor;

public partial class SettingsUserControl : UserControl
{
    public event Action NavigateToMain;

    public SettingsUserControl()
    {
        InitializeComponent();
    }

    private void TextBlockBack_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        NavigateToMain?.Invoke();
    }

    private void TextBlockNewThread_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        viewModel.Topics.Insert(0, new TopicViewModel());
    }

    private void TextBlockFavoriteThread_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var clickedTopic = sender as TextBlock;
        var tag = clickedTopic?.Tag ?? "";

        var topic = viewModel.Topics.FirstOrDefault(t => string.Equals(t.TopicName, (string)tag, StringComparison.OrdinalIgnoreCase));
        topic.IsFavoriteTopic = !topic.IsFavoriteTopic;

        var items = viewModel.AllNotificationItems.Where(x => string.Equals(x.TopicName, topic.TopicName, StringComparison.OrdinalIgnoreCase));
        
        foreach (var item in items)
        {
            item.IsFavoriteTopic = topic.IsFavoriteTopic;
        }
    }

    private void TextBlockFavoriteUser_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var clickedTextBox = sender as TextBlock;
        var tag = clickedTextBox.Tag as string;

        var user = viewModel.Users.FirstOrDefault(u => u.UserName == tag);
        user.Favorite = !user.Favorite;
        user.IgnoredUser = false;

        var items = viewModel.AllNotificationItems.Where(x => string.Equals(x.UserName, user.UserName, StringComparison.OrdinalIgnoreCase));

        foreach (var item in items)
        {
            item.IsFavoriteUser = user.Favorite;
        }
    }

    private void TextBlockNewUser_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        viewModel.Users.Insert(0, new UserViewModel());
    }

    private void DeleteThreadImage_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var image = sender as Image;
        var topicName = image.Tag as string;
        var topicToRemove = viewModel.Topics.FirstOrDefault(topic => topic.TopicName == topicName);
        viewModel.Topics.Remove(topicToRemove);
    }

    private void VIPUserImage_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var image = sender as Image;
        var userName = image.Tag as string;

        var user = viewModel.Users.FirstOrDefault(u => u.UserName == userName);
        user.VipUser = !user.VipUser;
        user.IgnoredUser = false;
    }

    private void IgnoreUserImage_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var image = sender as Image;
        var userName = image.Tag as string;

        var user = viewModel.Users.FirstOrDefault(u => u.UserName == userName);
        user.IgnoredUser = !user.IgnoredUser;
        user.VipUser = false;
        user.Favorite = false;
    }

    private void DeleteUserImage_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var image = sender as Image;
        var userName = image.Tag as string;
        var userToRemove = viewModel.Users.FirstOrDefault(u => u.UserName == userName);
        viewModel.Users.Remove(userToRemove);
    }

    private void TextBlockForum_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var textBlock = sender as TextBlock;
        var forumName = textBlock.Text;
        var forumItem = viewModel.ForumItems.FirstOrDefault(f => f.Name == forumName);
        forumItem.IsChecked = !forumItem.IsChecked;
    }

    private void TextBlockSelectAllForums_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var textBlock = sender as TextBlock;
        var checkbox = GetSelectAllCheckbox(textBlock);
        checkbox.IsChecked = !checkbox.IsChecked;
    }


    private static CheckBox GetSelectAllCheckbox(TextBlock textBlock)
    {
        var parent = textBlock.Parent;

        if (parent is StackPanel panel)
        {
            var index = panel.Children.IndexOf(textBlock);
            if (index > 0 && panel.Children[index - 1] is CheckBox checkBox)
            {
                return checkBox;
            }
        }

        return null;
    }

    private void TextBlockForum_PointerEntered(object sender, PointerEventArgs e)
    {
        var forumTextBlock = sender as TextBlock;
        Application.Current.TryGetResource("ForumItem_Selected_Foreground", Application.Current.ActualThemeVariant, out var selectedForeground);
        forumTextBlock.Foreground = new SolidColorBrush(Color.Parse(selectedForeground.ToString()));
    }

    private void TextBlockForum_PointerExited(object sender, PointerEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var forumTextBlock = sender as TextBlock;
        var textBlock = sender as TextBlock;
        var forumName = textBlock.Text;
        var forumItem = viewModel.ForumItems.FirstOrDefault(f => f.Name == forumName);
        Application.Current.TryGetResource("ForumItem_Foreground", Application.Current.ActualThemeVariant, out var foreground);
        if (!forumItem.IsChecked)
        {
            forumTextBlock.Foreground = new SolidColorBrush(Color.Parse(foreground.ToString()));
        }
    }

    private void ButtonCancel_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NavigateToMain?.Invoke();
    }

    private void ButtonSave_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        viewModel.SaveSettingsCommand.Execute(null);
        NavigateToMain?.Invoke();
    }

    private void ToggleSwitch_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Application.Current.ActualThemeVariant == ThemeVariant.Light)
        {
            App.Current.RequestedThemeVariant = ThemeVariant.Dark;
            var viewModel = DataContext as MainWindowViewModel;
            viewModel.Theme = "Dark";
        }
        else
        {
            App.Current.RequestedThemeVariant = ThemeVariant.Light;
            var viewModel = DataContext as MainWindowViewModel;
            viewModel.Theme = "Light";
        }
    }
}