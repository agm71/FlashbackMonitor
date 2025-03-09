using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace FlashbackMonitor.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter as string == "UserName" && !string.IsNullOrWhiteSpace(value?.ToString()))
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
