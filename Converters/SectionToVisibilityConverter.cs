using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using library.Models;

namespace library.Converters
{
    public class SectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NavSection section && parameter is string expected)
            {
                return section.ToString().Equals(expected, StringComparison.OrdinalIgnoreCase)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
