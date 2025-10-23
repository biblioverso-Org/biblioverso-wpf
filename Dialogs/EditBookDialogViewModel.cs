using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using library.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace library.Dialogs
{
    public partial class EditBookDialogViewModel : ObservableObject
    {
        [ObservableProperty] private string? titulo;
        [ObservableProperty] private string? autoresTexto;
        [ObservableProperty] private string? isbn;

        // ====== PDF Digital ======
        [ObservableProperty] private string? pdfUrl;       // URL en BD
        [ObservableProperty] private string? pdfLocalPath; // ruta local seleccionada
        [ObservableProperty] private string? pdfStatus;
        [ObservableProperty] private bool isPdfReady;

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

        public string? PdfFileName =>
    string.IsNullOrEmpty(PdfLocalPath) ? null : System.IO.Path.GetFileName(PdfLocalPath);


        partial void OnTituloChanged(string? v) => OnPropertyChanged(nameof(IsValid));
        partial void OnIsbnChanged(string? v) => OnPropertyChanged(nameof(IsValid));
       
        partial void OnPdfLocalPathChanged(string? value)
        {
            OnPropertyChanged(nameof(PdfFileName));
            IsPdfReady = !string.IsNullOrEmpty(value);
            PdfStatus = null;
        }
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

        [RelayCommand]
        private void SelectPdf()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                Title = "Seleccionar archivo PDF"
            };

            if (dialog.ShowDialog() == true)
                PdfLocalPath = dialog.FileName;
        }

        [RelayCommand]
        private async Task UploadPdfAsync()
        {
            if (string.IsNullOrEmpty(PdfLocalPath) || !System.IO.File.Exists(PdfLocalPath))
            {
                PdfStatus = "⚠️ Selecciona un archivo PDF primero.";
                return;
            }

            try
            {
                PdfStatus = "☁️ Subiendo PDF...";
                var cloud = new CloudinaryService("dvw5h3ccw", "893598289963378", "mKNQQGTlypYx947y0F72jpnzb88");
                var uploaded = await cloud.UploadPdfAsync(PdfLocalPath, "libros_pdf");

                if (!string.IsNullOrEmpty(uploaded))
                {
                    PdfUrl = uploaded;
                    PdfStatus = "✅ PDF subido correctamente.";
                    IsPdfReady = false;
                }
                else
                {
                    PdfStatus = "❌ Error al subir el PDF.";
                }
            }
            catch (Exception ex)
            {
                PdfStatus = $"❌ Error: {ex.Message}";
            }
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
            PdfUrl = l.PdfUrl,
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
