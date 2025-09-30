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
using System.Windows.Data;

namespace library.ViewModels
{
    public partial class OrdersViewModel : ObservableObject
    {
        private readonly ReservaService _reservaService = new();
        private readonly PrestamoService _prestamoService = new();
        private readonly StockService _stockService = new();
        private readonly NotificacionService _notifService = new();

        public ObservableCollection<Reserva> Orders { get; }
        public ICollectionView OrdersView { get; }

        [ObservableProperty] private string? searchText;
        [ObservableProperty] private string status = "all";

        public IAsyncRelayCommand<Reserva?> StartLoanCommand { get; }
        public IAsyncRelayCommand<Reserva?> CancelReservationCommand { get; }

        public OrdersViewModel()
        {
            Orders = new ObservableCollection<Reserva>();
            OrdersView = CollectionViewSource.GetDefaultView(Orders);
            OrdersView.Filter = FilterOrder;

            StartLoanCommand = new AsyncRelayCommand<Reserva?>(StartLoanAsync);
            CancelReservationCommand = new AsyncRelayCommand<Reserva?>(CancelReservationAsync);

            CargarReservasAsync();
        }

        private async Task CargarReservasAsync()
        {
            Orders.Clear();
            var reservas = await _reservaService.ObtenerReservasAsync();
            foreach (var r in reservas)
                Orders.Add(r);
            OrdersView.Refresh();
        }

        private bool FilterOrder(object obj)
        {
            if (obj is not Reserva r) return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim();
                bool hit =
                    (r.Usuario?.NombreCompleto?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Libro?.Titulo?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
                if (!hit) return false;
            }

            if (!string.Equals(Status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(r.Estado, Status, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        // ===== Confirmar préstamo =====
        private async Task StartLoanAsync(Reserva? r)
        {
            if (r is null) return;

            var vm = new LoanReservationDialogViewModel(r);
            var view = new LoanReservationDialog { DataContext = vm };

            var result = await DialogHost.Show(view, "RootDialog");
            if (result is not LoanReservationDialogViewModel m)
                return;

            // Buscar stock disponible para el libro
            var stockDisponible = await _stockService.ObtenerStockDisponibleAsync(r.IdLibro);
            if (stockDisponible is null)
            {
                // ❌ No hay stock → se queda en lista de espera (reserva sigue pendiente)
                await _notifService.CrearNotificacionAsync(
                    r.IdUsuario,
                    "⏳ Reserva en espera",
                    $"El libro \"{r.Libro?.Titulo}\" está en lista de espera. Se te notificará cuando haya ejemplares disponibles."
                );
                return;
            }

            // Bloqueamos stock como prestado
            await _stockService.MarcarComoPrestadoAsync(stockDisponible.IdStock);

            // Creamos préstamo
            await _prestamoService.CrearPrestamoAsync(
            r.IdUsuario,
            r.IdLibro,
            stockDisponible.IdStock,
            m.LoanStart,
            m.DueDate
        );


            // Actualizamos la reserva
            await _reservaService.CompletarReservaAsync(r.IdReserva);
            r.Estado = "completada";

            // Notificación al usuario
            await _notifService.CrearNotificacionAsync(
                r.IdUsuario,
                "📚 Préstamo confirmado",
                $"Has retirado el libro \"{r.Libro?.Titulo}\". Fecha de devolución: {m.DueDate:dd/MM/yyyy}"
            );

            OrdersView.Refresh();
        }

        private async Task CancelReservationAsync(Reserva? r)
        {
            if (r is null) return;

            var vm = new CancelReservationDialogViewModel(r);
            var view = new CancelReservationDialog { DataContext = vm };

            var result = await DialogHost.Show(view, "RootDialog");
            if (result is not CancelReservationDialogViewModel)
                return;

            // Cambiamos estado en BD
            await _reservaService.CancelarReservaAsync(r.IdReserva);
            r.Estado = "cancelada";

            // Notificación al usuario
            await _notifService.CrearNotificacionAsync(
                r.IdUsuario,
                "❌ Reserva cancelada",
                $"Tu reserva del libro \"{r.Libro?.Titulo}\" ha sido cancelada."
            );

            // Liberar stock reservado (si existía)
            var stockReservado = await _stockService.ObtenerStockReservadoPorUsuarioAsync(r.IdUsuario, r.IdLibro);
            if (stockReservado is not null)
                await _stockService.LiberarStockAsync(stockReservado.IdStock);

            OrdersView.Refresh();
        }
    }
}
