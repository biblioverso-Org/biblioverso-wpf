using System;

namespace library.Models
{
    public class HistorialPrestamo
    {
        public long IdHistorial { get; set; }
        public long IdReserva { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string Estado { get; set; } = "";
    }
}
