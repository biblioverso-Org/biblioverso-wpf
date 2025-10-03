using System;

namespace library.Models
{
    public class Notificacion
    {
        public long IdNotificacion { get; set; }
        public int IdUsuario { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool Leida { get; set; }

        // Iniciales calculadas si no hay foto
        public string Iniciales { get; set; } = "??";

        public string TimeAgo => (DateTime.Now - Fecha).TotalMinutes < 60
            ? $"{(int)(DateTime.Now - Fecha).TotalMinutes} min ago"
            : Fecha.ToString("g");
    }
}
