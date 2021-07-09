using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace p15.ValueConverters
{
    public class BoolToWrapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = (bool)value;

            return flag ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
