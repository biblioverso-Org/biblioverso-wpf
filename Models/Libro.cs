using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library.Models
{
    public class Libro
    {
        public long IdLibro { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Portada { get; set; }
        public string? Sinopsis { get; set; }

        // 🔄 cambiamos genero → Categoria
        public int? IdCategoria { get; set; }
        public Categoria? Categoria { get; set; }

        public string? Editorial { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Eliminado { get; set; } = false;

        public string? PdfUrl { get; set; }

        // Relaciones
        public List<Autor> Autores { get; set; } = new();
        public List<Stock> Stocks { get; set; } = new();
        public List<Opinion> Opiniones { get; set; } = new();

        public string AutoresTexto =>
     Autores != null && Autores.Any(a => a.Nombre != "Desconocido")
         ? string.Join(", ", Autores.Where(a => a.Nombre != "Desconocido").Select(a => a.Nombre))
         : "Sin autor registrado";

        public string SinopsisTexto =>
      !string.IsNullOrWhiteSpace(Sinopsis) ? Sinopsis : "Sinopsis no disponible";

        
        public string EditorialTexto =>
            !string.IsNullOrWhiteSpace(Editorial) ? Editorial : "Editorial desconocida";


    }

    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
