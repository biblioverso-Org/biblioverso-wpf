using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using library.Services;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

        public ObservableCollection<DashStat> Stats { get; }
        public IRelayCommand GenerateLoansReportCommand { get; }
        public IAsyncRelayCommand<Prestamo?> MarkReturnedCommand { get; }
        public IAsyncRelayCommand<Prestamo?> MarkOverdueCommand { get; }

        public LoanViewModel()
        {
            _prestamoService = new PrestamoService();

            Loans = new ObservableCollection<Prestamo>();
            LoansView = CollectionViewSource.GetDefaultView(Loans);
            LoansView.Filter = FilterLoan;

       
            _ = CargarPrestamosAsync();
        }

        // ====================== Cargar préstamos ======================
        private async Task CargarPrestamosAsync()
        {
            Loans.Clear();
            var prestamos = await _prestamoService.ObtenerPrestamosAsync();
            foreach (var p in prestamos)
                Loans.Add(p);

            ActualizarStats();
            LoansView.Refresh();
        }

        private void ActualizarStats()
        {
        }

        // ====================== Filtros ======================
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

        // ====================== Marcar como devuelto ======================
        private async Task MarkReturnedAsync(Prestamo? p)
        {
            
        }

        // ====================== Marcar como vencido ======================
        private async Task MarkOverdueAsync(Prestamo? p)
        {
        }

        // ====================== Reporte ======================
        private void GenerateReport()
        {
            System.Windows.MessageBox.Show($"📊 Reporte generado con {Loans.Count} préstamos.");
        }

        // ====================== Hooks para filtros ======================
        partial void OnSearchTextChanged(string? value) => LoansView.Refresh();
        partial void OnStatusChanged(string value) => LoansView.Refresh();
        partial void OnDateFromChanged(DateTime? value) => LoansView.Refresh();
        partial void OnDateToChanged(DateTime? value) => LoansView.Refresh();
    }
}
