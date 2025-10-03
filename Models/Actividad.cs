using System;

namespace library.Models
{
    public class Actividad
    {
        public int IdActividad { get; set; }
        public int IdUsuario { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
