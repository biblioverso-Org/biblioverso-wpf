using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace library.ViewModels
{
    public partial class AuthorDialogViewModel : ObservableObject
    {
        [ObservableProperty] private int idAutor;
        [ObservableProperty] private string nombre = string.Empty;
        [ObservableProperty] private string? nacionalidad;
        [ObservableProperty] private DateTime? fechaNac;
        [ObservableProperty] private DateTime? fechaMuerte;
        [ObservableProperty] private string? biografia;

        public AuthorDialogViewModel() { }

        public AuthorDialogViewModel(Autor autor)
        {
            IdAutor = autor.IdAutor;
            Nombre = autor.Nombre;
            Nacionalidad = autor.Nacionalidad;
            FechaNac = autor.FechaNac;
            FechaMuerte = autor.FechaMuerte;
            Biografia = autor.Biografia;
        }

        [RelayCommand]
        private void Save()
        {
            var autor = new Autor
            {
                IdAutor = IdAutor,
                Nombre = Nombre,
                Nacionalidad = Nacionalidad,
                FechaNac = FechaNac,
                FechaMuerte = FechaMuerte,
                Biografia = Biografia
            };

            DialogHost.Close("RootDialog", autor);
        }
    }
}
