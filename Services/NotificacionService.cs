using library.Data;
using library.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library.Services
{
    public class NotificacionService
    {
        private readonly Conexion _conexion = new Conexion();

        /// <summary>
        /// Crear una nueva notificación
        /// </summary>
        public async Task<long> CrearNotificacionAsync(int idUsuario, string titulo, string mensaje)
        {
            var query = @"INSERT INTO notificacion (id_usuario, titulo, mensaje, fecha, leida)
                          VALUES (@usuario, @titulo, @mensaje, now(), FALSE)
                          RETURNING id_notificacion";

            var param = new[]
            {
                new NpgsqlParameter("@usuario", idUsuario),
                new NpgsqlParameter("@titulo", titulo),
                new NpgsqlParameter("@mensaje", mensaje)
            };

            return (long)(await _conexion.ExecuteScalarAsync(query, param))!;
        }

        /// <summary>
        /// Obtener las últimas notificaciones de un usuario
        /// </summary>
        public async Task<List<Notificacion>> ObtenerNotificacionesAsync(int idUsuario, int limit = 20)
        {
            var query = @"SELECT id_notificacion, id_usuario, titulo, mensaje, leida, fecha
                          FROM notificacion
                          WHERE id_usuario=@usuario
                          ORDER BY fecha DESC
                          LIMIT @limite";

            var param = new[]
            {
                new NpgsqlParameter("@usuario", idUsuario),
                new NpgsqlParameter("@limite", limit)
            };

            var lista = new List<Notificacion>();
            using var reader = await _conexion.ExecuteReaderAsync(query, param);

            while (await reader.ReadAsync())
            {
                lista.Add(new Notificacion
                {
                    IdNotificacion = reader.GetInt64(0),
                    IdUsuario = reader.GetInt32(1),
                    Titulo = reader.GetString(2),
                    Mensaje = reader.GetString(3),
                    Leida = reader.GetBoolean(4),
                    Fecha = reader.GetDateTime(5)
                });
            }

            return lista;
        }

        /// <summary>
        /// Marcar una notificación como leída
        /// </summary>
        public async Task MarcarLeidaAsync(long idNotificacion)
        {
            var query = @"UPDATE notificacion SET leida=TRUE WHERE id_notificacion=@id";
            var param = new[] { new NpgsqlParameter("@id", idNotificacion) };
            await _conexion.ExecuteNonQueryAsync(query, param);
        }

        /// <summary>
        /// Marcar todas las notificaciones de un usuario como leídas
        /// </summary>
        public async Task MarcarTodasLeidasAsync(int idUsuario)
        {
            var query = @"UPDATE notificacion SET leida=TRUE WHERE id_usuario=@usuario";
            var param = new[] { new NpgsqlParameter("@usuario", idUsuario) };
            await _conexion.ExecuteNonQueryAsync(query, param);
        }
    }
}
