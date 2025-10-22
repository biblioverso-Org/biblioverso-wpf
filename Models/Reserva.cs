using System;

namespace library.Models
{
    public class Reserva
    {
        public long IdReserva { get; set; }
        public int IdUsuario { get; set; }
        public long IdLibro { get; set; }
        public DateTime FechaReserva { get; set; }
        public string Estado { get; set; } = "pendiente";
        // pendiente | notificado | cancelada | completada
        public int Cantidad { get; set; } = 1;
        // Relaciones
        public Usuario? Usuario { get; set; }
        public Libro? Libro { get; set; }
    }
}
