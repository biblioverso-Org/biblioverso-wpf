using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using library.Services; // GoogleBooksService, CategoriaService
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace library.Dialogs.AddBook
{
    public partial class AddBookDialogViewModel : ObservableObject
    {
        private readonly GoogleBooksService _googleBooks = new();
        private readonly CategoriaService _categoriaService = new();

        [ObservableProperty] private int stepIndex = 0;
        public bool IsStepSearch => StepIndex == 0;
        public bool IsStepDetails => StepIndex == 1;
        public bool IsStepDistribution => StepIndex == 2;

        partial void OnStepIndexChanged(int value)
        {
            OnPropertyChanged(nameof(IsStepSearch));
            OnPropertyChanged(nameof(IsStepDetails));
            OnPropertyChanged(nameof(IsStepDistribution));
            OnPropertyChanged(nameof(CanSave));
        }

        // ======= Campos de Libro =======
        [ObservableProperty] private string? titulo;
        [ObservableProperty] private string? autoresTexto;
        [ObservableProperty] private string? isbn;

        // 🔄 Normalizado: categoría (id + nombre)
        [ObservableProperty] private int? selectedCategoriaId;
        [ObservableProperty] private string? editorial;

        [ObservableProperty] private DateTime? fechaPublicacion;
        [ObservableProperty] private int stock = 1;
        [ObservableProperty] private double rating;
        [ObservableProperty] private bool disponible = true;
        [ObservableProperty] private string? portada;
        [ObservableProperty] private string? sinopsis;

        // ======= Distribución =======
        [ObservableProperty] private int distNew;
        [ObservableProperty] private int distUsed;
        [ObservableProperty] private int distWorn;
        [ObservableProperty] private int distDamaged;
        [ObservableProperty] private int distRepaired;
        [ObservableProperty] private int distRestoring;

        // 🔔 Recalcular cuando cambien las cantidades
        partial void OnDistNewChanged(int value) => UpdateDistribution();
        partial void OnDistUsedChanged(int value) => UpdateDistribution();
        partial void OnDistWornChanged(int value) => UpdateDistribution();
        partial void OnDistDamagedChanged(int value) => UpdateDistribution();
        partial void OnDistRepairedChanged(int value) => UpdateDistribution();
        partial void OnDistRestoringChanged(int value) => UpdateDistribution();

        private void UpdateDistribution()
        {
            OnPropertyChanged(nameof(DistTotal));
            OnPropertyChanged(nameof(DistDiff));
            OnPropertyChanged(nameof(CanSave));
        }

        public int DistTotal =>
            DistNew + DistUsed + DistWorn + DistDamaged + DistRepaired + DistRestoring;

        public int DistDiff => Stock - DistTotal;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Titulo) &&
            !string.IsNullOrWhiteSpace(Isbn) &&
            SelectedCategoriaId.HasValue;

        public bool CanSave =>
            IsValid && DistDiff == 0 && StepIndex == 2;

        // ======= Sugerencias =======
        public ObservableCollection<BookPick> Suggestions { get; }
        public ICollectionView SuggestionsView { get; }
        [ObservableProperty] private BookPick? selectedSuggestion;
        [ObservableProperty] private string searchText = string.Empty;

        // ======= Categorías (desde BD) =======
        public ObservableCollection<Categoria> Categorias { get; } = new();

        public AddBookDialogViewModel()
        {
            Suggestions = new ObservableCollection<BookPick>();
            SuggestionsView = CollectionViewSource.GetDefaultView(Suggestions);
            SuggestionsView.Filter = Filter;

            _ = LoadInitialSuggestionsAsync();
            _ = LoadCategoriasAsync();
        }

        private async Task LoadCategoriasAsync()
        {
            Categorias.Clear();
            var cats = await _categoriaService.GetCategoriasAsync();
            foreach (var c in cats) Categorias.Add(c);
        }

        private async Task LoadInitialSuggestionsAsync()
        {
            var results = await _googleBooks.SearchAsync("popular books");
            Suggestions.Clear();
            foreach (var r in results) Suggestions.Add(r);
            SuggestionsView.Refresh();
        }

        private bool Filter(object obj)
        {
            if (obj is not BookPick s) return false;

            var q = (SearchText ?? "").Trim();
            if (q.Length > 0 &&
                !(Contains(s.Titulo, q) || Contains(s.AutoresTexto, q) || Contains(s.ISBN, q)))
                return false;

            return true;
        }

        private static bool Contains(string? src, string term) =>
            !string.IsNullOrWhiteSpace(src) &&
            src.Contains(term, StringComparison.OrdinalIgnoreCase);

        partial void OnSearchTextChanged(string value) => _ = SearchBooksAsync(value);

        private async Task SearchBooksAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return;
            var results = await _googleBooks.SearchAsync(query);

            Suggestions.Clear();
            foreach (var r in results) Suggestions.Add(r);
            SuggestionsView.Refresh();
        }

        [RelayCommand]
        private void SelectSuggestion(BookPick? s)
        {
            if (s is null) return;

            foreach (var it in Suggestions) it.IsSelected = false;
            s.IsSelected = true;
            SelectedSuggestion = s;

            Titulo = s.Titulo;
            AutoresTexto = s.AutoresTexto;
            Isbn = s.ISBN;
            Editorial = s.Editorial;
            FechaPublicacion = s.FechaPublicacion;
            Stock = 1;
            Rating = s.Rating;
            Disponible = true;
            Portada = s.Portada;
            Sinopsis = s.Sinopsis;

            StepIndex = 1;
            OnPropertyChanged(nameof(CanSave));
        }

        [RelayCommand] private void OpenDistribution() => StepIndex = 2;
        [RelayCommand] private void Back() { if (StepIndex > 0) StepIndex--; }

        [RelayCommand]
        private void CreateAuthor()
        {
            if (string.IsNullOrWhiteSpace(AutoresTexto))
                AutoresTexto = "Nuevo autor";
            else
                AutoresTexto = AutoresTexto.Trim();

            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(CanSave));
        }

        // ✅ Conversión de ViewModel → Libro (para guardarlo en DB)
        public Libro ToLibro()
        {
            var libro = new Libro
            {
                Titulo = Titulo ?? string.Empty,
                ISBN = Isbn ?? string.Empty,
                IdCategoria = SelectedCategoriaId,
                Editorial = Editorial,
                FechaPublicacion = FechaPublicacion,
                Portada = Portada,
                Sinopsis = Sinopsis,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow,
                Autores = string.IsNullOrWhiteSpace(AutoresTexto)
                    ? new List<Autor>()
                    : AutoresTexto.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => new Autor { Nombre = a.Trim() })
                        .ToList(),
                Stocks = new List<Stock>()
                    .Concat(Enumerable.Repeat(new Stock { Disponibilidad = true, Estado = "Nuevo", Ubicacion = "General" }, DistNew))
                    .Concat(Enumerable.Repeat(new Stock { Disponibilidad = true, Estado = "Usado", Ubicacion = "General" }, DistUsed))
                    .Concat(Enumerable.Repeat(new Stock { Disponibilidad = true, Estado = "Desgastado", Ubicacion = "General" }, DistWorn))
                    .Concat(Enumerable.Repeat(new Stock { Disponibilidad = true, Estado = "Dañado", Ubicacion = "General" }, DistDamaged))
                    .Concat(Enumerable.Repeat(new Stock { Disponibilidad = true, Estado = "Reparado", Ubicacion = "General" }, DistRepaired))
                    .Concat(Enumerable.Repeat(new Stock { Disponibilidad = true, Estado = "En restauración", Ubicacion = "General" }, DistRestoring))
                    .ToList()
            };

            return libro;
        }
    }

    // DTO para búsqueda en Google Books
    public partial class BookPick : ObservableObject
    {
        public string Titulo { get; }
        public string AutoresTexto { get; }
        public string ISBN { get; }
        public string Categoria { get; }
        public string? Editorial { get; }
        public DateTime? FechaPublicacion { get; }
        public double Rating { get; }
        public string? Portada { get; }
        public string? Sinopsis { get; }

        [ObservableProperty] private bool isSelected;

        public BookPick(string titulo, string autoresTexto, string isbn,
                        string categoria, string? editorial, DateTime? fechaPublicacion,
                        double rating, string? portada, string? sinopsis)
        {
            Titulo = titulo;
            AutoresTexto = autoresTexto;
            ISBN = isbn;
            Categoria = categoria;
            Editorial = editorial;
            FechaPublicacion = fechaPublicacion;
            Rating = rating;
            Portada = portada;
            Sinopsis = sinopsis;
        }
    }
}
