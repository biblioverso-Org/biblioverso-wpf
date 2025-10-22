using library.Data;
using library.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library.Services
{
    public class PrestamoService
    {
        private readonly Conexion _conexion = new Conexion();

        public async Task<List<Prestamo>> ObtenerPrestamosAsync()
        {
            var query = @"
                SELECT p.id_prestamo, p.id_usuario, p.id_libro, p.id_stock,
                       p.fecha_prestamo, p.fecha_devolucion, p.fecha_vencimiento, p.estado,
                       u.nombre, u.apellido, u.usuario, u.foto,
                       l.titulo, l.portada
                FROM prestamo p
                INNER JOIN usuario u ON u.id_usuario = p.id_usuario
                INNER JOIN libro l ON l.id_libro = p.id_libro
                ORDER BY p.fecha_prestamo DESC;";

            var lista = new List<Prestamo>();

            using var reader = await _conexion.ExecuteReaderAsync(query);
            while (await reader.ReadAsync())
            {
                var p = new Prestamo
                {
                    IdPrestamo = reader.GetInt64(0),
                    IdUsuario = reader.GetInt32(1),
                    IdLibro = reader.GetInt64(2),
                    IdStock = reader.GetInt64(3),
                    FechaPrestamo = reader.GetDateTime(4),
                    FechaDevolucion = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    FechaVencimiento = reader.GetDateTime(6),
                    Estado = reader.GetString(7),
                    Usuario = new Usuario
                    {
                        IdUsuario = reader.GetInt32(1),
                        Nombre = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        Apellido = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        UserName = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        Foto = reader.IsDBNull(11) ? null : reader.GetString(11)
                    },
                    Libro = new Libro
                    {
                        IdLibro = reader.GetInt64(2),
                        Titulo = reader.IsDBNull(12) ? "" : reader.GetString(12),
                        Portada = reader.IsDBNull(13) ? null : reader.GetString(13)
                    }
                };
                lista.Add(p);
            }

            return lista;
        }

        public async Task DevolverPrestamoAsync(long idPrestamo)
        {
            var query = @"
                UPDATE prestamo 
                SET estado='devuelto', fecha_devolucion=now()
                WHERE id_prestamo=@id AND estado='activo';";

            await _conexion.ExecuteNonQueryAsync(query, new[] {
                new NpgsqlParameter("@id", idPrestamo)
            });
        }

        public async Task MarcarComoPerdidoAsync(long idPrestamo, string? motivo, decimal? monto)
        {
            var query = @"
                UPDATE prestamo 
                SET estado='perdido', fecha_devolucion=now()
                WHERE id_prestamo=@id AND estado='activo';";

            await _conexion.ExecuteNonQueryAsync(query, new[] {
                new NpgsqlParameter("@id", idPrestamo)
            });

            // ⚙️ Los triggers fn_prestamo_perdido() manejarán automáticamente:
            // - Multa
            // - Notificación al usuario
            // - Notificación al administrador
        }

        public async Task DevolverTodosAsync(int idUsuario)
        {
            var query = @"
        UPDATE prestamo
        SET estado='devuelto', fecha_devolucion=NOW()
        WHERE id_usuario=@id
          AND estado='activo';";

            await _conexion.ExecuteNonQueryAsync(query, new[] {
        new NpgsqlParameter("@id", idUsuario)
    });
        }

    }
}
