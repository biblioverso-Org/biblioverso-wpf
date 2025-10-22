using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using library.Models;

namespace library.Converters
{
    public class UserToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string nombre && App.Current is App app)
            {
                var vm = app.MainWindow?.DataContext as library.ViewModels.LoanViewModel;
                var usuario = vm?.Loans.FirstOrDefault(p => p.Usuario?.NombreCompleto == nombre)?.Usuario;
                return usuario?.Foto ?? "/Assets/Images/avatar.png";
            }
            return "/Assets/Images/avatar.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
