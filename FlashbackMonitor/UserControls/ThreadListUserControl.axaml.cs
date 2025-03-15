using Avalonia.Controls;
using FlashbackMonitor.Services;
using FlashbackMonitor.ViewModels;
using System;

namespace FlashbackMonitor;

public partial class ThreadListUserControl : UserControl
{
    public event Action NavigateToMain;

    public event Action<string, string> NavigateToTopic;

    public ThreadsListUserControlViewModel ViewModel { get; }
    public string ForumUrl { get; set; }

    public ThreadListUserControl(string forumUrl)
    {
        InitializeComponent();
        ViewModel = new ThreadsListUserControlViewModel(new FlashbackService(), forumUrl);
        DataContext = ViewModel;
        ForumUrl = forumUrl;
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
        NavigateToMain?.Invoke();
    }

    private void TopicNameTextBlock_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (sender is TextBlock clickedTextBlock)
            {
                var tagValue = clickedTextBlock.Tag?.ToString();
                NavigateToTopic?.Invoke("https://www.flashback.org" + tagValue, ForumUrl);
            }
        }
    }
}