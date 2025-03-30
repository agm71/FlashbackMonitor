using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using FlashbackMonitor.Services;
using FlashbackMonitor.Utils;
using FlashbackMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashbackMonitor;

public partial class TopicUserControl : UserControl
{
    private bool _disposed;

    public event Action NavigateToMain;
    public event Action<Stack<NavigationInfo>> NavigateToThreadList;

    public TopicUserControlViewModel ViewModel { get; }

    public Stack<NavigationInfo> NavigationInfo { get; set; }

    public TopicUserControl()
    {
    }

    public TopicUserControl(Stack<NavigationInfo> navigationInfo)
    {
        InitializeComponent();
        ViewModel = new TopicUserControlViewModel(new FlashbackService(), navigationInfo.First().RequestedUrl);
        DataContext = ViewModel;
        NavigationInfo = navigationInfo;
        this.KeyUp += TopicUserControl_KeyUp;
    }

    private void TopicUserControl_KeyUp(object sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.A || e.Key == Avalonia.Input.Key.Left)
        {
            PreviousPage();
        }
        else if (e.Key == Avalonia.Input.Key.D || e.Key == Avalonia.Input.Key.Right)
        {
            NextPage();
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromLogicalTree(e);
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();

        if (DataContext is TopicUserControlViewModel viewModel)
        {
            await ViewModel.InitializeAsync();

            if (NavigationInfo.First().RequestedUrl.EndsWith("n"))
            {
                var scrollViewer = this.FindControl<ScrollViewer>("SV");
                scrollViewer.ScrollToEnd();
            }
        }
    }

    private void BackImage_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        NavigationInfo.Pop();

        if (NavigationInfo.Count == 0 || NavigationInfo.First().RequestedUserControl == UserControlType.MainUserControl)
        {
            NavigateToMain?.Invoke();
        }
        else
        {
            NavigateToThreadList?.Invoke(NavigationInfo);
        }

    }

    private void TextBlockTopicText_PointerEntered(object sender, Avalonia.Input.PointerEventArgs e)
    {
        TextBlock hoveredTextBlock = sender as TextBlock;
        if (hoveredTextBlock != null)
        {
            if (hoveredTextBlock.Text.StartsWith("http"))
            {
                hoveredTextBlock.Foreground = new SolidColorBrush(Color.Parse("#ff0000"));
                hoveredTextBlock.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
            }
        }
    }

    private void TextBlockTopicText_PointerExited(object sender, Avalonia.Input.PointerEventArgs e)
    {
        TextBlock hoveredTextBlock = sender as TextBlock;

        if (hoveredTextBlock != null)
        {
            if (hoveredTextBlock.Text.StartsWith("http"))
            {
                hoveredTextBlock.Foreground = new SolidColorBrush(Color.Parse("Gray"));
                hoveredTextBlock.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (ViewModel.TopicPage != null)
            {
                ViewModel.TopicPage.PostItems.Clear();
                ViewModel.TopicPage = null;
            }
        }

        _disposed = true;
    }

    private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.FirstRun || ViewModel.TopicPage == null)
            return;

        var comboBox = sender as ComboBox;

        string url = ViewModel.TopicUrl.Replace("n", "");

        if (comboBox.SelectedIndex == 0)
        {
            if (url.LastIndexOf("p") > 5)
            {
                url = url.Substring(0, url.LastIndexOf("p"));
            }
        }
        else
        {
            if (url.LastIndexOf("p") > 5)
            {
                url = url.Substring(0, url.LastIndexOf("p"));
            }
            url = url + "p" + (comboBox.SelectedIndex + 1);
        }

        ViewModel.TopicUrl = url;

        await ViewModel.InitializeAsync();

        var scrollViewer = this.FindControl<ScrollViewer>("SV");
        scrollViewer.ScrollToHome();
    }

    private void SpoilerButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = sender as Button;
        var parentStackPanel = button.GetVisualParent() as StackPanel;
        var itemsControl = GetNextSibling(parentStackPanel);

        if (itemsControl != null)
        {
            itemsControl.IsVisible = !itemsControl.IsVisible;
        }
    }

    private Visual GetNextSibling(Visual element)
    {
        var parent = element.GetVisualParent();

        if (parent == null)
            return null;

        var children = parent.GetVisualChildren();

        bool foundElement = false;

        foreach (var child in children)
        {
            if (foundElement)
            {
                return child;
            }

            if (child == element)
            {
                foundElement = true;
            }
        }

        return null;
    }

    private void PreviousPageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PreviousPage();
    }

    private void NextPageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NextPage();
    }

    private async void RetryGettingPageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private void PreviousPage()
    {
        if (ViewModel.TopicPage != null && ViewModel.TopicPage.CurrentPage.ToString() != ViewModel.TopicPage.PageNumbers.First() && !ViewModel.IsLoading)
        {
            var pageNumberComboBox = this.FindControl<ComboBox>("PageNumberComboBox");
            PageNumberComboBox.SelectedIndex = PageNumberComboBox.SelectedIndex - 1;
        }
    }

    private void NextPage()
    {
        if (ViewModel.TopicPage != null && ViewModel.TopicPage.CurrentPage.ToString() != ViewModel.TopicPage.PageNumbers.Last() && !ViewModel.IsLoading)
        {
            var pageNumberComboBox = this.FindControl<ComboBox>("PageNumberComboBox");
            PageNumberComboBox.SelectedIndex = PageNumberComboBox.SelectedIndex + 1;
        }
    }

    private void UserControl_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Focus();
    }
}