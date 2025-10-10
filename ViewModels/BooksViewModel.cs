using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Abstractions;
using library.Dialogs;
using library.Dialogs.AddBook;
using library.Models;
using library.Services;
using MahApps.Metro.IconPacks;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace library.ViewModels
{
    public partial class BooksViewModel : ObservableObject, ISearchable
    {
        private readonly ILibroService _service;

        public ObservableCollection<Libro> Libros { get; }
        public ICollectionView LibrosView { get; }

        public IAsyncRelayCommand AddBookAsyncCommand { get; }
        public IAsyncRelayCommand<Libro?> EditBookCommand { get; }
        public IAsyncRelayCommand<Libro?> DeleteBookCommand { get; }
        public IAsyncRelayCommand<Libro?> ManageStockCommand { get; }

        private string _search = string.Empty;

        public ObservableCollection<DashStat> Stats { get; }

        [ObservableProperty] private string availability = "all";
        [ObservableProperty] private int selectedCategoriaId = -1; // ✅ ahora filtramos por categoría
        [ObservableProperty] private double minRating = 0;

        public IRelayCommand GenerateBooksReportCommand { get; }

        public BooksViewModel()
        {
            _service = new LibroService();
            Libros = new ObservableCollection<Libro>();

            LibrosView = CollectionViewSource.GetDefaultView(Libros);
            LibrosView.Filter = Filter;

            AddBookAsyncCommand = new AsyncRelayCommand(AddBookAsync);
            EditBookCommand = new AsyncRelayCommand<Libro?>(EditBookAsync);
            DeleteBookCommand = new AsyncRelayCommand<Libro?>(DeleteBookAsync);
            GenerateBooksReportCommand = new RelayCommand(GenerateReport);
            ManageStockCommand = new AsyncRelayCommand<Libro?>(ManageStockAsync);
            Stats = new ObservableCollection<DashStat>();
            _ = LoadLibrosAsync();
        }

        // ===================== Cargar libros =====================
        private async Task LoadLibrosAsync()
        {
            Libros.Clear();
            var libros = await _service.GetLibrosAsync();
            foreach (var l in libros) Libros.Add(l);

        }

        private async Task ManageStockAsync(Libro? libro)
        {
            if (libro is null) return;

            var vm = new ManageStockDialogViewModel(libro, _service);
            var view = new ManageStockDialog { DataContext = vm };

            var result = await DialogHost.Show(view, "RootDialog");
            if (result is ManageStockDialogViewModel m)
            {
                await m.GuardarCambiosAsync();
                await LoadLibrosAsync();
            }
        }

        // ===================== Filtro búsqueda =====================
        private bool Filter(object obj)
        {
            if (obj is not Libro l) return false;

            var t = _search.Trim();

            // 🔎 Texto libre
            if (!string.IsNullOrWhiteSpace(t))
            {
                if (!(Contains(l.Titulo, t) ||
                      Contains(l.ISBN, t) ||
                      Contains(l.Editorial, t) ||
                      Contains(l.Categoria?.Nombre, t) || // ✅ ahora usa categoría
                      (l.FechaPublicacion?.Year.ToString().Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                      l.Autores.Any(a => Contains(a.Nombre, t))))
                {
                    return false;
                }
            }

            // 📌 Filtro por categoría
            if (SelectedCategoriaId > 0 && l.IdCategoria != SelectedCategoriaId)
                return false;

            // 📌 Filtro por rating mínimo
            if (l.Opiniones.Any() && l.Opiniones.Average(o => o.Calificacion) < MinRating)
                return false;

            return true;
        }

        private static bool Contains(string? s, string term)
            => !string.IsNullOrEmpty(s) && s.Contains(term, StringComparison.OrdinalIgnoreCase);

        public void SetSearch(string? text)
        {
            _search = text ?? string.Empty;
            LibrosView.Refresh();
        }

        // ===================== Agregar libro =====================
        private async Task AddBookAsync()
        {
            var vm = new AddBookDialogViewModel();
            var view = new AddBookDialog { DataContext = vm };

            var result = await DialogHost.Show(view, "RootDialog");
            if (result is AddBookDialogViewModel m)
            {
                var nuevo = m.ToLibro();
                await _service.AddLibroCompletoAsync(nuevo);
                await LoadLibrosAsync();
            }
        }

        // ===================== Editar libro =====================
        private async Task EditBookAsync(Libro? l)
        {
            if (l is null) return;

            var vm = EditBookDialogViewModel.FromLibro(l);
            var view = new EditBookDialog { DataContext = vm };

            var result = await DialogHost.Show(view, "RootDialog");

            if (result is EditBookDialogViewModel m)
            {
                var cloudService = new CloudinaryService(
                    "dvw5h3ccw", // tu cloud name
                    "893598289963378", // tu API key
                    "mKNQQGTlypYx947y0F72jpnzb88" // tu API secret
                );

                string? portadaUrl = l.Portada;

                // 📌 Solo si cargó nueva imagen local
                if (!string.IsNullOrEmpty(m.PortadaUrl) && System.IO.File.Exists(m.PortadaUrl))
                    portadaUrl = await cloudService.UploadImageAsync(m.PortadaUrl);

                l.Titulo = m.Titulo ?? l.Titulo;
                l.ISBN = m.Isbn ?? l.ISBN;
                l.Editorial = m.Editorial ?? l.Editorial;
                l.FechaPublicacion = m.FechaPublicacion ?? l.FechaPublicacion;
                l.Sinopsis = m.Sinopsis ?? l.Sinopsis;
                l.Portada = portadaUrl;
                l.IdCategoria = m.SelectedCategoriaId > 0 ? m.SelectedCategoriaId : l.IdCategoria;

                if (!string.IsNullOrWhiteSpace(m.AutoresTexto))
                {
                    l.Autores = m.AutoresTexto
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => new Autor { Nombre = a.Trim() })
                        .ToList();
                }

                await _service.UpdateLibroAsync(l);
                await LoadLibrosAsync();
            }
        }

        private void GenerateReport()
        {
            MessageBox.Show($"📊 Reporte generado con {Libros.Count} libros.");
        }

        // ===================== Eliminar libro =====================
        private async Task DeleteBookAsync(Libro? l)
        {
            if (l is null) return;

            var vm = new ConfirmDeleteBookDialogViewModel(l);
            var view = new ConfirmDeleteBookDialog { DataContext = vm };

            var result = await DialogHost.Show(view, "RootDialog");
            if (result is string action && action == "True")
            {
                await _service.DeleteLibroAsync(l.IdLibro);
                await LoadLibrosAsync();
            }
        }
    }
}
