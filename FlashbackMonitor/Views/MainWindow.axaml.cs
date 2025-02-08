using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;
using FlashbackMonitor.ViewModels;
using System.Linq;
using Avalonia.Media;

namespace FlashbackMonitor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBlockTopicName_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var url = textBlock.Tag as string;

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
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

            var topic = viewModel.Topics.FirstOrDefault(t => t.TopicName == (string)tag);
            if (topic != null)
            {
                topic.IsFavoriteTopic = !topic.IsFavoriteTopic;
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

        private void TextBlockForum_PointerEntered(object sender, PointerEventArgs e)
        {
            var forumTextBlock = sender as TextBlock;
            forumTextBlock.Foreground = new SolidColorBrush(Color.Parse("#e25fe4"));
        }

        private void TextBlockForum_PointerExited(object sender, PointerEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            var forumTextBlock = sender as TextBlock;
            var textBlock = sender as TextBlock;
            var forumName = textBlock.Text;
            var forumItem = viewModel.ForumItems.FirstOrDefault(f => f.Name == forumName);

            if (!forumItem.IsChecked)
            {
                forumTextBlock.Foreground = Brushes.Gray;
            }
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
    }
}