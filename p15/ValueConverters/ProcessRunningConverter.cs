using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace p15.ValueConverters
{
    public class ProcessRunningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isRunning = (bool)value;

            return isRunning
                ? new SolidColorBrush(Color.FromArgb(100, 0, 200, 0))
                : new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
