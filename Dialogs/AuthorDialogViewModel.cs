using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;

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

        // 🖼️ Nueva propiedad: ruta o URL de la foto
        [ObservableProperty] private string? foto;

        // 🖼️ Vista previa (local o desde Cloudinary)
        [ObservableProperty] private string? fotoPreview;

        public AuthorDialogViewModel() { }

        public AuthorDialogViewModel(Autor autor)
        {
            IdAutor = autor.IdAutor;
            Nombre = autor.Nombre;
            Nacionalidad = autor.Nacionalidad;
            FechaNac = autor.FechaNac;
            FechaMuerte = autor.FechaMuerte;
            Biografia = autor.Biografia;
            Foto = autor.Foto;
            FotoPreview = autor.Foto;
        }

        // 📸 Permitir al usuario seleccionar una imagen desde el explorador
        [RelayCommand]
        private void SelectImage()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Seleccionar imagen del autor",
                Filter = "Imágenes (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg"
            };

            if (dlg.ShowDialog() == true)
            {
                Foto = dlg.FileName;
                FotoPreview = dlg.FileName;
            }
        }

        // 💾 Guardar cambios
        [RelayCommand]
        private void Save()
        {
            DialogHost.Close("RootDialog", this);
        }

    }
}
