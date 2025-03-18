using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Avalonia.LogicalTree;
using Avalonia.Controls;

namespace FlashbackMonitor;

public class LazyImage : TemplatedControl
{
    private ScrollViewer _parentScrollViewer;

    public static readonly StyledProperty<string> ImageUrlProperty =
        AvaloniaProperty.Register<LazyImage, string>(nameof(ImageUrl));

    public static readonly StyledProperty<Bitmap> ImageSourceProperty =
        AvaloniaProperty.Register<LazyImage, Bitmap>(nameof(ImageSource));

    public static readonly StyledProperty<int> ImageWidthProperty =
        AvaloniaProperty.Register<LazyImage, int>(nameof(ImageWidth));

    public static readonly StyledProperty<int> ImageHeightProperty =
        AvaloniaProperty.Register<LazyImage, int>(nameof(ImageHeight));

    public static readonly StyledProperty<string> FromProperty =
        AvaloniaProperty.Register<LazyImage, string>(nameof(From));

    public string ImageUrl
    {
        get => GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public Bitmap ImageSource
    {
        get => GetValue(ImageSourceProperty);
        private set => SetValue(ImageSourceProperty, value);
    }

    public int ImageWidth
    {
        get => GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    public int ImageHeight
    {
        get => GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public string From
    {
        get => GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    private CancellationTokenSource _cancellationTokenSource;

    public LazyImage()
    {
        IsVisible = false;
        this.PropertyChanged += ImageLoaderControl_PropertyChanged;
    }

    private async void ImageLoaderControl_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ImageUrlProperty && e.NewValue is string url && !string.IsNullOrWhiteSpace(url))
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await LoadImageAsync(url, token);
            
            IsVisible = true;
        }
    }

    private async Task LoadImageAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = new HttpClient();

            cancellationToken.ThrowIfCancellationRequested();

            var imageBytes = await httpClient.GetByteArrayAsync(url, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            using var memoryStream = new MemoryStream(imageBytes);
            var bitmap = new Bitmap(memoryStream);

            cancellationToken.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(() => ImageSource = bitmap);

            ImageHeight = ImageSource.PixelSize.Height;

            FindParentScrollViewer();
        }
        catch
        {
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
        catch
        {
        }

        base.OnDetachedFromLogicalTree(e);
    }

    private void FindParentScrollViewer()
    {
        var parent = this.Parent;

        while (parent != null)
        {
            if (parent is ScrollViewer scrollViewer)
            {
                _parentScrollViewer = scrollViewer;
                break;
            }

            parent = parent.Parent;
        }

        if (_parentScrollViewer != null)
        {
            if (From == "main")
                _parentScrollViewer.ScrollToEnd();
            else
                _parentScrollViewer.ScrollToHome();
        }
    }
}