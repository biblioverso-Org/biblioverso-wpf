using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;

namespace library.Dialogs
{
    public partial class ConfirmDeleteBookDialogViewModel : ObservableObject
    {
        [ObservableProperty] private string? titulo;
        [ObservableProperty] private string? autores;
        [ObservableProperty] private string? isbn;
        [ObservableProperty] private string? categoria;
        [ObservableProperty] private int? anio;
        [ObservableProperty] private string? portada;

        public ConfirmDeleteBookDialogViewModel(Libro libro)
        {
            Titulo = libro.Titulo;
            Isbn = libro.ISBN;
            Categoria = libro.Categoria?.Nombre ; // ✅ compatibilidad
            Anio = libro.FechaPublicacion?.Year;
            Portada = libro.Portada;

            Autores = libro.Autores != null && libro.Autores.Any()
                ? string.Join(", ", libro.Autores.Select(a => a.Nombre))
                : "Desconocido";
        }
    }
}
