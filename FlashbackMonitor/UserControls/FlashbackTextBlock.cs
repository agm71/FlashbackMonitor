using Avalonia.Controls;
using Avalonia;
using Avalonia.Input;
using System.Diagnostics;
using Avalonia.Media;
using FlashbackMonitor.Services;

namespace FlashbackMonitor
{
    public class FlashbackTextBlock : TextBlock
    {
        public static readonly StyledProperty<TextKind> TextKindProperty =
            AvaloniaProperty.Register<FlashbackTextBlock, TextKind>("Text");

        public static readonly StyledProperty<string> AdditionalDataProperty =
            AvaloniaProperty.Register<FlashbackTextBlock, string>("Text");

        public static readonly StyledProperty<string> CustomTextProperty =
            AvaloniaProperty.Register<FlashbackTextBlock, string>(nameof(TextKind));

        public TextKind TextKind
        {
            get => GetValue(TextKindProperty);
            set => SetValue(TextKindProperty, value);
        }

        public string AdditionalData
        {
            get => GetValue(AdditionalDataProperty);
            set => SetValue(AdditionalDataProperty, value);
        }

        public override void ApplyTemplate()
        {
            if (TextKind == TextKind.Bold)
            {
                FontWeight = FontWeight.Bold;
            }
            else if (TextKind == TextKind.Italic)
            {
                FontStyle = FontStyle.Italic;
            }
            else if (TextKind == TextKind.Link)
            {
                Foreground = new SolidColorBrush(Color.Parse("#4b7bb7"));
                Cursor = Cursor.Parse("Hand");
                AddHandler(PointerReleasedEvent, OnPointerReleased, handledEventsToo: true);
            }
            else if (TextKind == TextKind.Bullet)
            {
                FontSize = 16;
                LineHeight = 14;
            }
            else if (TextKind == TextKind.Code)
            {
                Foreground = new SolidColorBrush(Color.Parse("#529543"));
            }
            else if (TextKind == TextKind.Smiley)
            {
                LineHeight = 16;
            }

            if (AdditionalData?.Contains("MarginBottom#") == true)
            {
                Margin = new Thickness(0, 0, 0, double.Parse(AdditionalData.Split('#')[1])); //= AdditionalData.Split('#')[1];
            }

            base.ApplyTemplate();
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            FlashbackTextBlock textBlock = (FlashbackTextBlock)sender;

            if (!e.Handled)
            {
                if (textBlock.TextKind == TextKind.Link)
                {
                    var url = textBlock.AdditionalData;

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        OpenUrl(url);
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        private static void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
