using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace p15.ValueConverters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            return ((DateTime)value).ToString("yyyy/MM/dd HH:mm:ss.fff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
