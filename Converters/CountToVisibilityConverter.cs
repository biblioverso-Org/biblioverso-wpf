using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace library.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                if (parameter?.ToString() == "HasItems")
                    return count > 0 ? Visibility.Visible : Visibility.Collapsed;

                if (parameter?.ToString() == "Empty")
                    return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
