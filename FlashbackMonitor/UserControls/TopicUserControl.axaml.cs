using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using FlashbackMonitor.Services;
using FlashbackMonitor.ViewModels;
using System;

namespace FlashbackMonitor;

public partial class TopicUserControl : UserControl
{
    private bool _disposed;

    public event Action NavigateToMain;
    public TopicUserControlViewModel ViewModel { get; }

    public string TopicUrl { get; set; }

    public TopicUserControl()
    {
        InitializeComponent();
        ViewModel = new TopicUserControlViewModel(new FlashbackService(), TopicUrl);
        DataContext = ViewModel;
    }

    public TopicUserControl(string topicUrl)
    {
        InitializeComponent();
        ViewModel = new TopicUserControlViewModel(new FlashbackService(), topicUrl);
        DataContext = ViewModel;
        TopicUrl = topicUrl;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromLogicalTree(e);
    }

    private void TextBlock_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        NavigateToMain?.Invoke();
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();

        if (DataContext is TopicUserControlViewModel viewModel)
        {
            await ViewModel.InitializeAsync();
            var scrollViewer = this.FindControl<ScrollViewer>("SV");
            scrollViewer.ScrollToEnd();
        }
    }

    private void BackImage_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        NavigateToMain?.Invoke();
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
    }
}