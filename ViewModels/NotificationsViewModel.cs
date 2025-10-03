using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using library.Services;

namespace library.ViewModels
{
    public partial class NotificationsViewModel : ObservableObject
    {
        private readonly NotificacionService _notificacionService = new();

        [ObservableProperty]
        private ObservableCollection<Notificacion> notificaciones = new();

        /// <summary>
        /// Cargar notificaciones de un usuario
        /// </summary>
        public async Task CargarNotificacionesAsync(int idUsuario)
        {
            var notifs = await _notificacionService.ObtenerNotificacionesAsync(idUsuario, 50);

            Notificaciones.Clear();
            foreach (var n in notifs.Where(n => !n.Leida)) // 🔹 solo mostrar no leídas
                Notificaciones.Add(n);
        }

        /// <summary>
        /// Marcar una notificación como leída y actualizar UI
        /// </summary>
        [RelayCommand]
        private async Task MarcarLeido(Notificacion notif)
        {
            if (notif == null) return;

            await _notificacionService.MarcarLeidaAsync(notif.IdNotificacion);

            // Eliminar de la lista al instante
            Notificaciones.Remove(notif);

            // 🔄 Actualizar contador en TopBar
            var shell = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive)?
                .DataContext as ShellViewModel;

            shell?.TopBar?.UpdateNotificationCount();
        }
    }
}
