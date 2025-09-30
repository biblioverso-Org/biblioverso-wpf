using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library.Models
{
    public class Opinion
    {
        public int IdOpinion { get; set; }
        public long IdLibro { get; set; }
        public int IdUsuario { get; set; }
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime Fecha { get; set; }
    }
}
