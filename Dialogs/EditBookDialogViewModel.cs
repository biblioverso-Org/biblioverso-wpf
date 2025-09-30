using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;

namespace library.Dialogs
{
    public partial class EditBookDialogViewModel : ObservableObject
    {
        [ObservableProperty] private string? titulo;
        [ObservableProperty] private string? autoresTexto;
        [ObservableProperty] private string? isbn;

        // Nuevo: categoría normalizada
        [ObservableProperty] private int? selectedCategoriaId;

        [ObservableProperty] private string? editorial;
        [ObservableProperty] private DateTime? fechaPublicacion;
        [ObservableProperty] private int stockCantidad = 1;
        [ObservableProperty] private double rating;
        [ObservableProperty] private bool disponible = true;

        // 👉 BD (URL de Cloudinary o null)
        [ObservableProperty] private string? portada;

        // 👉 UI (archivo local o URL)
        [ObservableProperty] private string? portadaUrl;

        [ObservableProperty] private string? sinopsis;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Titulo) &&
            !string.IsNullOrWhiteSpace(Isbn);

        partial void OnTituloChanged(string? v) => OnPropertyChanged(nameof(IsValid));
        partial void OnIsbnChanged(string? v) => OnPropertyChanged(nameof(IsValid));

        // Cuando cambia la portada
        partial void OnPortadaUrlChanged(string? v) => OnPropertyChanged(nameof(PortadaUrl));

        [RelayCommand]
        private void NewAuthor()
        {
            if (string.IsNullOrWhiteSpace(AutoresTexto))
                AutoresTexto = "Nuevo autor";
            else
                AutoresTexto = AutoresTexto.Trim();
        }

        [RelayCommand]
        private void UploadPortada()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imágenes (*.png;*.jpg)|*.png;*.jpg"
            };

            if (dialog.ShowDialog() == true)
                PortadaUrl = dialog.FileName;
        }

        [RelayCommand]
        private void RemovePortada()
        {
            PortadaUrl = null;
            Portada = null;
        }

        // ====== Mappers ======
        public static EditBookDialogViewModel FromLibro(Libro l) => new()
        {
            Titulo = l.Titulo,
            Isbn = l.ISBN,
            Editorial = l.Editorial,
            FechaPublicacion = l.FechaPublicacion,
            StockCantidad = l.Stocks.Sum(s => 1),
            Portada = l.Portada,
            PortadaUrl = l.Portada,
            Sinopsis = l.Sinopsis,
            Rating = l.Opiniones.Any() ? l.Opiniones.Average(o => o.Calificacion) : 0,
            Disponible = l.Stocks.Any(s => s.Disponibilidad),
            SelectedCategoriaId = l.IdCategoria, // ✅ categoría
            AutoresTexto = string.Join(", ", l.Autores.Select(a => a.Nombre))
        };

        public Libro ToLibro(Libro original)
        {
            original.Titulo = Titulo ?? original.Titulo;
            original.ISBN = Isbn ?? original.ISBN;
            original.Editorial = Editorial ?? original.Editorial;
            original.FechaPublicacion = FechaPublicacion;
            original.Portada = Portada;
            original.Sinopsis = Sinopsis;
            original.IdCategoria = SelectedCategoriaId ?? original.IdCategoria;

            if (!string.IsNullOrWhiteSpace(AutoresTexto))
            {
                original.Autores = AutoresTexto
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => new Autor { Nombre = a.Trim() })
                    .ToList();
            }

            return original;
        }
    }
}
