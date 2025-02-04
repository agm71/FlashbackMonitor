using Avalonia.Controls;
using System.Diagnostics;

namespace FlashbackMonitor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBlock_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var url = textBlock.Tag as string;

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}