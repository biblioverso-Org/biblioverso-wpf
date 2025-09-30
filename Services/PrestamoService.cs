using library.Data;
using library.Models;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace library.Services
{
    public class PrestamoService
    {
        private readonly Conexion _conexion = new Conexion();

        /// <summary>
        /// Crear un nuevo préstamo (cuando el usuario recoge un libro reservado).
        /// </summary>
        public async Task<long> CrearPrestamoAsync(int idUsuario, long idLibro, long idStock,DateTime loanStart, DateTime dueDate)
        {
            var query = @"INSERT INTO prestamo 
                  (id_usuario, id_libro, id_stock, fecha_prestamo, fecha_vencimiento, estado)
                  VALUES (@usuario, @libro, @stock, @fecha_prestamo, @fecha_vencimiento, 'activo')
                  RETURNING id_prestamo";

            var param = new[]
            {
                new NpgsqlParameter("@usuario", idUsuario),
                new NpgsqlParameter("@libro", idLibro),
                new NpgsqlParameter("@stock", idStock),
                new NpgsqlParameter("@fecha_prestamo", loanStart),
                new NpgsqlParameter("@fecha_vencimiento", dueDate)
            };

            return (long)(await _conexion.ExecuteScalarAsync(query, param))!;
        }

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
        ORDER BY p.fecha_prestamo DESC";

            var lista = new List<Prestamo>();

            using var reader = await _conexion.ExecuteReaderAsync(query);
            while (await reader.ReadAsync())
            {
                var prestamo = new Prestamo
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

                lista.Add(prestamo);
            }

            return lista;
        }

        /// <summary>
        /// Devolver un libro prestado. Marca el préstamo como devuelto y libera el stock.
        /// Si hay reservas pendientes → se asigna automáticamente.
        /// </summary>
        public async Task DevolverPrestamoAsync(long idPrestamo)
        {
            // 1. Buscar préstamo
            var queryFind = @"SELECT id_usuario, id_libro, id_stock
                              FROM prestamo
                              WHERE id_prestamo=@id AND estado='activo'";
            var paramFind = new[] { new NpgsqlParameter("@id", idPrestamo) };

            int idUsuario = 0;
            long idLibro = 0, idStock = 0;

            using (var reader = await _conexion.ExecuteReaderAsync(queryFind, paramFind))
            {
                if (await reader.ReadAsync())
                {
                    idUsuario = reader.GetInt32(0);
                    idLibro = reader.GetInt64(1);
                    idStock = reader.GetInt64(2);
                }
                else
                {
                    throw new InvalidOperationException("❌ Préstamo no encontrado o ya devuelto.");
                }
            }

            // 2. Marcar préstamo como devuelto
            var queryUpdatePrestamo = @"UPDATE prestamo 
                                        SET estado='devuelto', fecha_devolucion=now()
                                        WHERE id_prestamo=@id";
            await _conexion.ExecuteNonQueryAsync(queryUpdatePrestamo, paramFind);

            // 3. Revisar lista de espera
            var reservaService = new ReservaService();
            var reserva = await reservaService.ObtenerPrimeraReservaPendiente(idLibro);

            if (reserva != null)
            {
                // 3a. Notificar al usuario de la reserva
                await reservaService.NotificarReservaAsync(reserva.IdReserva);

                var notifService = new NotificacionService();
                await notifService.CrearNotificacionAsync(
                    reserva.IdUsuario,
                    "📚 Libro disponible",
                    "El libro que reservaste ya está disponible por devolución. Ven a recogerlo en las próximas 24h."
                );

                // 3b. Stock liberado se marca como reservado para esa reserva
                var queryBloquear = "UPDATE stock SET disponibilidad=FALSE WHERE id_stock=@idStock";
                await _conexion.ExecuteNonQueryAsync(queryBloquear, new[] {
                    new NpgsqlParameter("@idStock", idStock)
                });
            }
            else
            {
                // 3c. Si NO hay reservas → el stock vuelve a estar disponible
                var queryLiberar = "UPDATE stock SET disponibilidad=TRUE WHERE id_stock=@idStock";
                await _conexion.ExecuteNonQueryAsync(queryLiberar, new[] {
                    new NpgsqlParameter("@idStock", idStock)
                });
            }
        }
    }
}
