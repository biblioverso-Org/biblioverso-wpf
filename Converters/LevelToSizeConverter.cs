using System;
using System.Globalization;
using System.Windows.Data;

namespace library.Converters
{
    public class LevelToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double level && double.TryParse(parameter?.ToString(), out double baseSize))
            {
                // Mapea amplitud (0–1) a tamaño base + incremento visible
                double scale = 1 + (level * 0.5);
                return baseSize * scale;
            }
            return parameter ?? 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
