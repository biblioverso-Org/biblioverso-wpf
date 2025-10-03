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

        public async Task<List<Notificacion>> ObtenerNotificacionesAsync(int idUsuario, int limit = 20)
        {
            var query = @"SELECT n.id_notificacion, n.id_usuario, n.titulo, n.mensaje, n.leida, n.fecha,
                                 u.nombre, u.apellido
                          FROM notificacion n
                          JOIN usuario u ON n.id_usuario = u.id_usuario
                          WHERE n.id_usuario=@usuario
                          ORDER BY n.fecha DESC
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
                var notif = new Notificacion
                {
                    IdNotificacion = reader.GetInt64(0),
                    IdUsuario = reader.GetInt32(1),
                    Titulo = reader.GetString(2),
                    Mensaje = reader.GetString(3),
                    Leida = reader.GetBoolean(4),
                    Fecha = reader.GetDateTime(5)
                };

                // Generar iniciales a partir de nombre/apellido
                var nombre = reader.IsDBNull(6) ? "" : reader.GetString(6);
                var apellido = reader.IsDBNull(7) ? "" : reader.GetString(7);

                if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(apellido))
                    notif.Iniciales = $"{nombre[0]}{apellido[0]}".ToUpper();
                else if (!string.IsNullOrEmpty(nombre))
                    notif.Iniciales = nombre.Substring(0, 1).ToUpper();
                else
                    notif.Iniciales = "??";

                lista.Add(notif);
            }

            return lista;
        }

        public async Task MarcarLeidaAsync(long idNotificacion)
        {
            var query = @"UPDATE notificacion SET leida=TRUE WHERE id_notificacion=@id";
            var param = new[] { new NpgsqlParameter("@id", idNotificacion) };
            await _conexion.ExecuteNonQueryAsync(query, param);
        }

        public async Task<int> ContarNoLeidasAsync(int idUsuario)
        {
            var query = @"SELECT COUNT(*) FROM notificacion WHERE id_usuario=@usuario AND leida=FALSE";
            var param = new[] { new NpgsqlParameter("@usuario", idUsuario) };
            return Convert.ToInt32(await _conexion.ExecuteScalarAsync(query, param));
        }
    }
}
