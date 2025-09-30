using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library.Models
{
    public class Autor
    {
        public int IdAutor { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Nacionalidad { get; set; }
        public DateTime? FechaNac { get; set; }
        public DateTime? FechaMuerte { get; set; }
        public string? Biografia { get; set; }
    }
}
