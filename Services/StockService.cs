using library.Data;
using library.Models;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library.Services
{
    public class StockService
    {
        private readonly Conexion _conexion = new Conexion();

        public async Task<Stock?> ObtenerStockDisponibleAsync(long idLibro)
        {
            var query = @"SELECT id_stock, id_libro, ubicacion, disponibilidad, estado, situacion
                      FROM stock
                      WHERE id_libro=@libro AND disponibilidad=TRUE AND situacion='Disponible'
                      LIMIT 1";

            var param = new[] { new NpgsqlParameter("@libro", idLibro) };
            using var reader = await _conexion.ExecuteReaderAsync(query, param);

            if (await reader.ReadAsync())
            {
                return new Stock
                {
                    IdStock = reader.GetInt64(0),
                    IdLibro = reader.GetInt64(1),
                    Ubicacion = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Disponibilidad = reader.GetBoolean(3),
                    Estado = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Situacion = reader.IsDBNull(5) ? "Disponible" : reader.GetString(5)
                };
            }

            return null;
        }

        public async Task MarcarComoReservadoAsync(long idStock)
        {
            var query = "UPDATE stock SET disponibilidad=FALSE, situacion='Reservado' WHERE id_stock=@id";
            await _conexion.ExecuteNonQueryAsync(query, new[] { new NpgsqlParameter("@id", idStock) });
        }

        public async Task MarcarComoPrestadoAsync(long idStock)
        {
            var query = "UPDATE stock SET disponibilidad=FALSE, situacion='Prestado' WHERE id_stock=@id";
            await _conexion.ExecuteNonQueryAsync(query, new[] { new NpgsqlParameter("@id", idStock) });
        }

        public async Task LiberarStockAsync(long idStock)
        {
            var query = "UPDATE stock SET disponibilidad=TRUE, situacion='Disponible' WHERE id_stock=@id";
            await _conexion.ExecuteNonQueryAsync(query, new[] { new NpgsqlParameter("@id", idStock) });
        }

        public async Task<Stock?> ObtenerStockReservadoPorUsuarioAsync(int idUsuario, long idLibro)
        {
            var query = @"
                SELECT s.id_stock, s.id_libro, s.ubicacion, s.disponibilidad, s.estado, s.situacion
                FROM stock s
                INNER JOIN reserva r ON r.id_libro = s.id_libro
                WHERE r.id_usuario=@usuario
                  AND r.id_libro=@libro
                  AND r.estado='pendiente'
                  AND s.situacion='Reservado'
                LIMIT 1";

            var param = new[]
            {
                new NpgsqlParameter("@usuario", idUsuario),
                new NpgsqlParameter("@libro", idLibro)
            };

            using var reader = await _conexion.ExecuteReaderAsync(query, param);
            if (await reader.ReadAsync())
            {
                return new Stock
                {
                    IdStock = reader.GetInt64(0),
                    IdLibro = reader.GetInt64(1),
                    Ubicacion = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Disponibilidad = reader.GetBoolean(3),
                    Estado = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Situacion = reader.IsDBNull(5) ? "Reservado" : reader.GetString(5)
                };
            }

            return null;
        }
    }

}
