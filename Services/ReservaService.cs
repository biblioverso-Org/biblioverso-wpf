using library.Data;
using library.Models;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace library.Services
{
    public class ReservaService
    {
        private readonly Conexion _conexion = new Conexion();

        /// <summary>
        /// Crea una nueva reserva en estado "pendiente".
        /// </summary>
        public async Task<long> CrearReservaAsync(int idUsuario, long idLibro)
        {
            var query = @"INSERT INTO reserva (id_usuario, id_libro, fecha_reserva, estado)
                          VALUES (@usuario, @libro, now(), 'pendiente')
                          RETURNING id_reserva";

            var param = new[]
            {
                new NpgsqlParameter("@usuario", idUsuario),
                new NpgsqlParameter("@libro", idLibro)
            };

            return (long)(await _conexion.ExecuteScalarAsync(query, param))!;
        }


        public async Task<List<Reserva>> ObtenerReservasAsync()
        {
            var query = @"
        SELECT r.id_reserva, r.id_usuario, r.id_libro, r.fecha_reserva, r.estado, r.cantidad,
               u.nombre, u.apellido, u.usuario, u.foto,
               l.titulo, l.portada
        FROM reserva r
        INNER JOIN usuario u ON u.id_usuario = r.id_usuario
        INNER JOIN libro l ON l.id_libro = r.id_libro
        ORDER BY r.fecha_reserva DESC";

            var lista = new List<Reserva>();

            using var reader = await _conexion.ExecuteReaderAsync(query);

            while (await reader.ReadAsync())
            {
                var reserva = new Reserva
                {
                    IdReserva = reader.GetInt64(0),
                    IdUsuario = reader.GetInt32(1),
                    IdLibro = reader.GetInt64(2),
                    FechaReserva = reader.GetDateTime(3),
                    Estado = reader.GetString(4),
                    Cantidad = reader.IsDBNull(5) ? 1 : reader.GetInt32(5), // ✅ cantidad correcta

                    Usuario = new Usuario
                    {
                        IdUsuario = reader.GetInt32(1),
                        Nombre = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        Apellido = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        UserName = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        Foto = reader.IsDBNull(9) ? null : reader.GetString(9)
                    },
                    Libro = new Libro
                    {
                        IdLibro = reader.GetInt64(2),
                        Titulo = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        Portada = reader.IsDBNull(11) ? null : reader.GetString(11)
                    }
                };

                lista.Add(reserva);
            }

            return lista;
        }


        /// <summary>
        /// Obtiene la primera reserva pendiente para un libro (orden FIFO).
        /// </summary>
        public async Task<Reserva?> ObtenerPrimeraReservaPendiente(long idLibro)
        {
            var query = @"SELECT id_reserva, id_usuario, id_libro, fecha_reserva, estado
                          FROM reserva
                          WHERE id_libro=@libro AND estado='pendiente'
                          ORDER BY fecha_reserva ASC
                          LIMIT 1";

            var param = new[] { new NpgsqlParameter("@libro", idLibro) };

            using var reader = await _conexion.ExecuteReaderAsync(query, param);
            if (await reader.ReadAsync())
            {
                return new Reserva
                {
                    IdReserva = reader.GetInt64(0),
                    IdUsuario = reader.GetInt32(1),
                    IdLibro = reader.GetInt64(2),
                    FechaReserva = reader.GetDateTime(3),
                    Estado = reader.GetString(4)
                };
            }
            return null;
        }

        /// <summary>
        /// Marca la reserva como "notificado".
        /// </summary>
        public async Task RecogerReservaAsync(long idReserva)
        {
            var query = @"UPDATE reserva
                          SET estado='recoger'
                          WHERE id_reserva=@id";
            var param = new[] { new NpgsqlParameter("@id", idReserva) };
            await _conexion.ExecuteNonQueryAsync(query, param);
        }

        /// <summary>
        /// Marca la reserva como "completada" (cuando el usuario recoge el libro).
        /// </summary>
        public async Task CompletarReservaAsync(long idReserva)
        {
            var query = @"UPDATE reserva
                          SET estado='completado'
                          WHERE id_reserva=@id";
            var param = new[] { new NpgsqlParameter("@id", idReserva) };
            await _conexion.ExecuteNonQueryAsync(query, param);
        }

        /// <summary>
        /// Cancela la reserva y libera al siguiente en la lista de espera.
        /// </summary>
        public async Task CancelarReservaAsync(long idReserva)
        {
            var query = @"UPDATE reserva
                          SET estado='cancelado'
                          WHERE id_reserva=@id";
            var param = new[] { new NpgsqlParameter("@id", idReserva) };
            await _conexion.ExecuteNonQueryAsync(query, param);
        }

        public async Task MarcarReservaComoListaAsync(long idReserva)
        {
            var query = @"UPDATE reserva
                  SET estado='recoger'
                  WHERE id_reserva=@id";
            var param = new[] { new NpgsqlParameter("@id", idReserva) };
            await _conexion.ExecuteNonQueryAsync(query, param);
        }
    }
}
