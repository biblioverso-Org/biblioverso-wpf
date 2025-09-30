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

        // Para UI
        public string DisplayTitle => Titulo;
        public string DisplayMessage => Mensaje;

        public string TimeAgo => (DateTime.Now - Fecha).TotalMinutes < 60
            ? $"{(int)(DateTime.Now - Fecha).TotalMinutes} min ago"
            : Fecha.ToString("g");

        public string Avatar => "Assets/default-avatar.png"; // 🔹 si no tienes avatar real
    }
}
