using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
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

            if (value is bool boolean)
            {
                switch (type)
                {
                    case "vipuser":
                        return boolean
                            ? new Bitmap("Assets/crown.png")
                            : new Bitmap("Assets/crown_gray.png");
                    case "ignoreduser":
                        return boolean
                            ? new Bitmap("Assets/stop.png")
                            : new Bitmap("Assets/stop_gray.png");
                    case "favoriteuser":
                        return boolean
                            ? "#f99404"
                            : "Gray";
                    case "favoritetopic":
                        return boolean
                            ? "#f99404"
                            : "Gray";
                    case "forumname":
                        return boolean
                            ? "#cc9d42"
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