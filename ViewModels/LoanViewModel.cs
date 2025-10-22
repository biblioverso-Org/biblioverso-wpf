using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Dialogs;
using library.Models;
using library.Services;
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
    public partial class LoanViewModel : ObservableObject
    {
        private readonly PrestamoService _prestamoService;

        public ObservableCollection<Prestamo> Loans { get; }
        public ICollectionView LoansView { get; }

        [ObservableProperty] private string? searchText;
        [ObservableProperty] private string status = "all";
        [ObservableProperty] private DateTime? dateFrom;
        [ObservableProperty] private DateTime? dateTo;

        // 🔹 Comandos principales
        public IAsyncRelayCommand<Prestamo?> MarkReturnedCommand { get; }
        public IAsyncRelayCommand<Prestamo?> MarkLostCommand { get; }
        public IAsyncRelayCommand<Prestamo?> ShowInfoCommand { get; }
        public IAsyncRelayCommand<int> ReturnAllCommand { get; }

        public LoanViewModel()
        {
            _prestamoService = new PrestamoService();
            Loans = new ObservableCollection<Prestamo>();
            LoansView = CollectionViewSource.GetDefaultView(Loans);
            LoansView.Filter = FilterLoan;

            MarkReturnedCommand = new AsyncRelayCommand<Prestamo?>(MarkReturnedAsync);
            MarkLostCommand = new AsyncRelayCommand<Prestamo?>(MarkLostAsync);
            ShowInfoCommand = new AsyncRelayCommand<Prestamo?>(ShowInfoAsync);
            ReturnAllCommand = new AsyncRelayCommand<int>(ReturnAllAsync);

            _ = CargarPrestamosAsync();
        }

        // ====================== Cargar préstamos ======================
        private async Task CargarPrestamosAsync()
        {
            Loans.Clear();
            var prestamos = await _prestamoService.ObtenerPrestamosAsync();
            foreach (var p in prestamos)
                Loans.Add(p);

            // ✅ Agrupación por usuario
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(Loans);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("Usuario.NombreCompleto"));
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("Usuario.NombreCompleto", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("FechaPrestamo", ListSortDirection.Descending));

            LoansView.Refresh();
        }

        // ====================== Filtro ======================
        private bool FilterLoan(object obj)
        {
            if (obj is not Prestamo p) return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim();
                bool hit =
                    (p.Usuario?.NombreCompleto?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Libro?.Titulo?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
                if (!hit) return false;
            }

            if (!string.Equals(Status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(p.Estado, Status, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (DateFrom.HasValue && p.FechaPrestamo.Date < DateFrom.Value.Date) return false;
            if (DateTo.HasValue && p.FechaPrestamo.Date > DateTo.Value.Date) return false;

            return true;
        }

        // ====================== Ver información ======================
        private async Task ShowInfoAsync(Prestamo? p)
        {
            if (p is null) return;

            try
            {
                var dialog = new LoanInfoDialog { DataContext = p };
                await DialogHost.Show(dialog, "RootDialog");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Error al mostrar información:\n{ex.Message}");
            }
        }

        // ====================== Devolver un libro ======================
        private async Task MarkReturnedAsync(Prestamo? p)
        {
            if (p is null) return;

            try
            {
                // 🟦 Abre diálogo de devolución
                var dialogVm = new LoanReturnDialogViewModel(p);
                var dialog = new LoanReturnDialog { DataContext = dialogVm };
                var result = await DialogHost.Show(dialog, "RootDialog");

                if (result is Prestamo)
                {
                    await _prestamoService.DevolverPrestamoAsync(p.IdPrestamo);
                    p.Estado = "devuelto";
                    LoansView.Refresh();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"⚠️ Error al devolver el libro:\n{ex.Message}");
            }
        }


        // ====================== Marcar como perdido ======================
        private async Task MarkLostAsync(Prestamo? p)
        {
            if (p is null) return;

            try
            {
                var vm = new LoanLostDialogViewModel(p);
                var dialog = new LoanLostDialog { DataContext = vm };
                var result = await DialogHost.Show(dialog, "RootDialog");

                if (result is Prestamo)
                {
                    await _prestamoService.MarcarComoPerdidoAsync(p.IdPrestamo, vm.Motivo, vm.Monto);
                    p.Estado = "perdido";
                    LoansView.Refresh();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"⚠️ Error al marcar el préstamo como perdido:\n{ex.Message}");
            }
        }

        // ====================== Devolver todos del usuario ======================
        private async Task ReturnAllAsync(int idUsuario)
        {
            try
            {
                var prestamosUsuario = Loans.Where(p => p.IdUsuario == idUsuario && p.Estado == "activo").ToList();

                if (!prestamosUsuario.Any())
                {
                    MessageBox.Show("No hay préstamos activos para este usuario.");
                    return;
                }

                var confirm = MessageBox.Show(
                    $"¿Deseas devolver los {prestamosUsuario.Count} préstamos activos de este usuario?",
                    "Confirmar devolución", MessageBoxButton.YesNo);

                if (confirm != MessageBoxResult.Yes) return;

                await _prestamoService.DevolverTodosAsync(idUsuario);

                foreach (var p in prestamosUsuario)
                    p.Estado = "devuelto";

                LoansView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Error al devolver todos:\n{ex.Message}");
            }
        }


        // ====================== Refresh automáticos ======================
        partial void OnSearchTextChanged(string? value) => LoansView.Refresh();
        partial void OnStatusChanged(string value) => LoansView.Refresh();
        partial void OnDateFromChanged(DateTime? value) => LoansView.Refresh();
        partial void OnDateToChanged(DateTime? value) => LoansView.Refresh();
    }
}
