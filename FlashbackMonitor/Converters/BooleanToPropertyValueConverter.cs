using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using FlashbackMonitor.Services;
using System;
using System.Globalization;

namespace FlashbackMonitor.Converters
{
    public class BooleanToPropertyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = parameter as string;

            if (value is TextKind textKind)
            {
                if (textKind == TextKind.Italic)
                {
                    return "Italic";
                }
                else if (textKind == TextKind.Bold)
                {
                    return "Bold";
                }
            }

            if (value is ThreadItem threadItem)
            {
                Application.Current.TryGetResource("TopicList_Item_Even_Background", Application.Current.ActualThemeVariant, out var evenBackground);
                Application.Current.TryGetResource("TopicList_Item_Odd_Background", Application.Current.ActualThemeVariant, out var oddBackground);
                Application.Current.TryGetResource("TopicList_Item_Pinned_Background", Application.Current.ActualThemeVariant, out var pinnedBackground);

                if (Application.Current!.ActualThemeVariant == ThemeVariant.Light)
                {
                    if (threadItem.PinnedThread)
                    {
                        return pinnedBackground;
                    }
                    else
                    {
                        return threadItem.Index % 2 == 0 ? evenBackground : oddBackground;
                    }
                }
                else
                {
                    if (threadItem.PinnedThread)
                    {
                        return pinnedBackground;
                    }
                    else
                    {
                        return threadItem.Index % 2 == 0 ? evenBackground : oddBackground;
                    }
                }
            }

            if (value is bool boolean)
            {
                switch (type)
                {
                    case "vipuser":
                        return boolean
                            ? new Bitmap(AssetLoader.Open(new Uri("avares://FlashbackMonitor/Assets/crown.png")))
                            : new Bitmap(AssetLoader.Open(new Uri("avares://FlashbackMonitor/Assets/crown_gray.png")));
                    case "ignoreduser":
                        return boolean
                            ? new Bitmap(AssetLoader.Open(new Uri("avares://FlashbackMonitor/Assets/stop.png")))
                            : new Bitmap(AssetLoader.Open(new Uri("avares://FlashbackMonitor/Assets/stop_gray.png")));
                    case "favoriteuser":
                        return boolean
                            ? "#f99404"
                            : "Gray";
                    case "favoritetopic":
                        return boolean
                            ? "#f99404"
                            : "Gray";
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}