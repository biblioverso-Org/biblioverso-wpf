using library.Data;
using library.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library.Services
{
    public class ActividadService
    {
        private readonly Conexion _conexion = new();

        public async Task<IEnumerable<Actividad>> GetAllAsync()
        {
            var actividades = new List<Actividad>();
            string query = @"
                SELECT a.id_actividad, a.id_usuario, u.usuario, r.nombre AS rol, a.accion, a.fecha
                FROM historial_actividad a
                JOIN usuario u ON a.id_usuario = u.id_usuario
                JOIN rol r ON u.id_rol = r.id_rol
                ORDER BY a.fecha DESC";

            await using var reader = await _conexion.ExecuteReaderAsync(query);
            while (await reader.ReadAsync())
            {
                actividades.Add(new Actividad
                {
                    IdActividad = reader.GetInt32(0),
                    IdUsuario = reader.GetInt32(1),
                    Usuario = reader.GetString(2),
                    Rol = reader.GetString(3),
                    Accion = reader.GetString(4),
                    Fecha = reader.GetDateTime(5)
                });
            }
            return actividades;
        }

        public async Task<IEnumerable<Actividad>> GetFilteredAsync(string? usuario, string? rol, DateTime? desde, DateTime? hasta)
        {
            var actividades = new List<Actividad>();
            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            string query = @"
                SELECT a.id_actividad, a.id_usuario, u.usuario, r.nombre AS rol, a.accion, a.fecha
                FROM historial_actividad a
                JOIN usuario u ON a.id_usuario = u.id_usuario
                JOIN rol r ON u.id_rol = r.id_rol
                WHERE 1=1";

            if (!string.IsNullOrEmpty(usuario))
            {
                query += " AND u.usuario ILIKE @usuario";
                parameters.Add(new NpgsqlParameter("usuario", $"%{usuario}%"));
            }

            if (!string.IsNullOrEmpty(rol))
            {
                query += " AND r.nombre ILIKE @rol";
                parameters.Add(new NpgsqlParameter("rol", $"%{rol}%"));
            }

            if (desde.HasValue)
            {
                query += " AND a.fecha >= @desde";
                parameters.Add(new NpgsqlParameter("desde", desde.Value));
            }

            if (hasta.HasValue)
            {
                query += " AND a.fecha <= @hasta";
                parameters.Add(new NpgsqlParameter("hasta", hasta.Value));
            }

            query += " ORDER BY a.fecha DESC";

            await using var reader = await _conexion.ExecuteReaderAsync(query, parameters.ToArray());
            while (await reader.ReadAsync())
            {
                actividades.Add(new Actividad
                {
                    IdActividad = reader.GetInt32(0),
                    IdUsuario = reader.GetInt32(1),
                    Usuario = reader.GetString(2),
                    Rol = reader.GetString(3),
                    Accion = reader.GetString(4),
                    Fecha = reader.GetDateTime(5)
                });
            }

            return actividades;
        }
    }
}
