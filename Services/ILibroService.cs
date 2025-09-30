using System.Collections.Generic;
using System.Threading.Tasks;
using library.Models;

namespace library.Services
{
    public interface ILibroService
    {
        Task<IEnumerable<Libro>> GetLibrosAsync();
        Task<Libro?> GetLibroByIdAsync(long id);
        Task<int> AddLibroAsync(Libro libro); // 🚨 puedes mantenerlo, pero ya no lo usaremos
        Task<int> AddLibroCompletoAsync(Libro libro); // ✅ flujo completo
        Task<int> UpdateLibroAsync(Libro libro);
        Task<int> DeleteLibroAsync(long id);

        // Extra
        Task<IEnumerable<Libro>> BuscarEnGoogleBooksAsync(string query);

      
        Task UpdateStockAsync(long idLibro, Dictionary<string, int> cambios);


    }
}
