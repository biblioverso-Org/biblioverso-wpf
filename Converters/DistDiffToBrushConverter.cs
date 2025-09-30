using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace library.Converters
{
    public class DistDiffToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int diff)
            {
                // Verde si está balanceado (0), Rojo si no
                return diff == 0
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")) // Verde éxito
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")); // Rojo error
            }

            return Brushes.Gray; // fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
