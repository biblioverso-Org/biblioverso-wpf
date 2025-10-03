using library.Data;
using library.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library.Services
{
    public class AutorService
    {
        private readonly Conexion _conexion = new();

        public async Task<IEnumerable<Autor>> GetAllAsync()
        {
            var autores = new List<Autor>();
            var query = "SELECT id_autor, nombre, nacionalidad, fecha_nac, fecha_muerte, biografia FROM autor ORDER BY id_autor";

            await using var reader = await _conexion.ExecuteReaderAsync(query);
            while (await reader.ReadAsync())
            {
                autores.Add(new Autor
                {
                    IdAutor = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Nacionalidad = reader.IsDBNull(2) ? null : reader.GetString(2),
                    FechaNac = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    FechaMuerte = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Biografia = reader.IsDBNull(5) ? null : reader.GetString(5),
                });
            }

            return autores;
        }

        public async Task<Autor?> GetByIdAsync(int id)
        {
            var query = "SELECT id_autor, nombre, nacionalidad, fecha_nac, fecha_muerte, biografia FROM autor WHERE id_autor=@id";
            var param = new[] { new NpgsqlParameter("id", id) };

            await using var reader = await _conexion.ExecuteReaderAsync(query, param);
            if (await reader.ReadAsync())
            {
                return new Autor
                {
                    IdAutor = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Nacionalidad = reader.IsDBNull(2) ? null : reader.GetString(2),
                    FechaNac = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    FechaMuerte = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Biografia = reader.IsDBNull(5) ? null : reader.GetString(5),
                };
            }

            return null;
        }

        public async Task AddAsync(Autor autor)
        {
            var query = @"
                INSERT INTO autor (nombre, nacionalidad, fecha_nac, fecha_muerte, biografia) 
                VALUES (@nombre, @nacionalidad, @fecha_nac, @fecha_muerte, @biografia)";

            var parameters = new[]
            {
                new NpgsqlParameter("nombre", autor.Nombre),
                new NpgsqlParameter("nacionalidad", (object?)autor.Nacionalidad ?? DBNull.Value),
                new NpgsqlParameter("fecha_nac", (object?)autor.FechaNac ?? DBNull.Value),
                new NpgsqlParameter("fecha_muerte", (object?)autor.FechaMuerte ?? DBNull.Value),
                new NpgsqlParameter("biografia", (object?)autor.Biografia ?? DBNull.Value),
            };

            await _conexion.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task UpdateAsync(Autor autor)
        {
            var query = @"
                UPDATE autor SET 
                    nombre=@nombre,
                    nacionalidad=@nacionalidad,
                    fecha_nac=@fecha_nac,
                    fecha_muerte=@fecha_muerte,
                    biografia=@biografia
                WHERE id_autor=@id";

            var parameters = new[]
            {
                new NpgsqlParameter("id", autor.IdAutor),
                new NpgsqlParameter("nombre", autor.Nombre),
                new NpgsqlParameter("nacionalidad", (object?)autor.Nacionalidad ?? DBNull.Value),
                new NpgsqlParameter("fecha_nac", (object?)autor.FechaNac ?? DBNull.Value),
                new NpgsqlParameter("fecha_muerte", (object?)autor.FechaMuerte ?? DBNull.Value),
                new NpgsqlParameter("biografia", (object?)autor.Biografia ?? DBNull.Value),
            };

            await _conexion.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task DeleteAsync(int id)
        {
            var query = "DELETE FROM autor WHERE id_autor=@id";
            var param = new[] { new NpgsqlParameter("id", id) };

            await _conexion.ExecuteNonQueryAsync(query, param);
        }
    }
}
