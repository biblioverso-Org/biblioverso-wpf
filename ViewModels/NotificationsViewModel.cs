using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;
using library.Services;

namespace library.ViewModels
{
    public partial class NotificationsViewModel : ObservableObject
    {
        private readonly NotificacionService _notificacionService = new();

        [ObservableProperty]
        private ObservableCollection<Notificacion> todayNotifications = new();

        [ObservableProperty]
        private ObservableCollection<Notificacion> weekNotifications = new();

        // ⚠️ Ajusta al id_usuario real del administrador
        private readonly int _adminId = 1;

        public async Task CargarNotificacionesAsync()
        {
            var notifs = await _notificacionService.ObtenerNotificacionesAsync(_adminId, 50);

            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek); // lunes

            TodayNotifications.Clear();
            WeekNotifications.Clear();

            foreach (var n in notifs)
            {
                if (n.Fecha.Date == hoy)
                    TodayNotifications.Add(n);
                else if (n.Fecha.Date >= inicioSemana)
                    WeekNotifications.Add(n);
            }
        }
    }
}
