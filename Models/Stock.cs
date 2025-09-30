using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library.Models
{
    public class Stock
    {
        public long IdStock { get; set; }
        public long IdLibro { get; set; }
        public string? Ubicacion { get; set; }
        public bool Disponibilidad { get; set; }
        public string Estado { get; set; } = "Nuevo";
        public string Situacion { get; set; } = "Disponible";
    }
}
