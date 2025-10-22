using System;

namespace library.Models
{
    public class Prestamo
    {
        public long IdPrestamo { get; set; }
        public int IdUsuario { get; set; }
        public long IdLibro { get; set; }
        public long IdStock { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = "activo";
        // activo | devuelto | vencido | perdido

        // Relaciones
        public Usuario? Usuario { get; set; }
        public Libro? Libro { get; set; }
        public Stock? Stock { get; set; }

        // Extras (para dialogs)
        public string? Notas { get; set; }
        public string? Motivo { get; set; }
        public decimal? Monto { get; set; }
        public bool NotificarUsuario { get; set; }
        public int Cantidad { get; set; } = 1;
        public int MaxCantidad { get; set; } = 1;
    }
}
