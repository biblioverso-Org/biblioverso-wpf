// Services/UsuarioService.cs
using library.Data;
using library.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library.Services
{
    public class UsuarioService
    {
        private readonly Conexion _conexion;

        public UsuarioService()
        {
            _conexion = new Conexion();
        }

        public async Task<List<Usuario>> GetClientesAsync()
        {
            var clientes = new List<Usuario>();

            using var conn = _conexion.CrearConexion();
            await conn.OpenAsync();

            var query = @"SELECT id_usuario, usuario, password, nombre, apellido, email, telefono, direccion, 
                                 genero, fecha_nac, nacionalidad, biografia, foto, id_rol
                          FROM usuario
                          WHERE id_rol = 2";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clientes.Add(new Usuario
                {
                    IdUsuario = reader.GetInt32(0),
                    UserName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Password = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Nombre = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Apellido = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Telefono = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Direccion = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    Genero = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    FechaNacimiento = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                    Nacionalidad = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    Biografia = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    Foto = reader.IsDBNull(12) ? null : reader.GetString(12),
                    IdRol = reader.IsDBNull(13) ? null : reader.GetInt32(13)
                });
            }

            return clientes;
        }

        public async Task<int> AddClienteAsync(Usuario u)
        {
            var query = @"INSERT INTO usuario 
                  (usuario, password, nombre, apellido, email, telefono, direccion, genero, fecha_nac, nacionalidad, biografia, id_rol, foto)
                  VALUES 
                  (@usuario, @password, @nombre, @apellido, @correo, @telefono, @direccion, @genero, @fecha_nac, @nacionalidad, @biografia, 2, @foto)";

            var parameters = new[]
            {
        new NpgsqlParameter("@usuario", u.UserName),
        new NpgsqlParameter("@password", u.Password),
        new NpgsqlParameter("@nombre", u.Nombre ?? ""),
        new NpgsqlParameter("@apellido", u.Apellido ?? ""),
        new NpgsqlParameter("@correo", u.Email ?? ""),
        new NpgsqlParameter("@telefono", u.Telefono ?? ""),
        new NpgsqlParameter("@direccion", u.Direccion ?? ""),
        new NpgsqlParameter("@genero", u.Genero ?? "Otro"),
        new NpgsqlParameter("@fecha_nac", (object?)u.FechaNacimiento ?? DBNull.Value),
        new NpgsqlParameter("@nacionalidad", u.Nacionalidad ?? ""),
        new NpgsqlParameter("@biografia", u.Biografia ?? ""),
        new NpgsqlParameter("@foto", u.Foto ?? "")
    };

            return await _conexion.ExecuteNonQueryAsync(query, parameters);
        }



        public async Task<int> UpdateClienteAsync(Usuario u)
        {
            var query = @"UPDATE usuario
                  SET usuario=@usuario, password=@password, nombre=@nombre, apellido=@apellido, 
                      email=@correo, telefono=@telefono, direccion=@direccion, genero=@genero,
                      fecha_nac=@fecha_nac, nacionalidad=@nacionalidad, biografia=@biografia, foto=@foto
                  WHERE id_usuario=@id";

            var parameters = new[]
            {
        new NpgsqlParameter("@usuario", u.UserName),
        new NpgsqlParameter("@password", u.Password ?? ""),
        new NpgsqlParameter("@nombre", u.Nombre ?? ""),
        new NpgsqlParameter("@apellido", u.Apellido ?? ""),
        new NpgsqlParameter("@correo", u.Email ?? ""),
        new NpgsqlParameter("@telefono", u.Telefono ?? ""),
        new NpgsqlParameter("@direccion", u.Direccion ?? ""),
        new NpgsqlParameter("@genero", u.Genero ?? "Otro"),
        new NpgsqlParameter("@fecha_nac", (object?)u.FechaNacimiento ?? DBNull.Value),
        new NpgsqlParameter("@nacionalidad", u.Nacionalidad ?? ""),
        new NpgsqlParameter("@biografia", u.Biografia ?? ""),
        new NpgsqlParameter("@foto", (object?)u.Foto ?? DBNull.Value),
        new NpgsqlParameter("@id", u.IdUsuario)
    };

            return await _conexion.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> DeleteClienteAsync(int id)
        {
            var query = @"DELETE FROM usuario WHERE id_usuario=@id";
            var parameters = new[] { new NpgsqlParameter("@id", id) };
            return await _conexion.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<List<HistorialPrestamo>> GetHistorialByUsuarioAsync(int idUsuario)
        {
            var historial = new List<HistorialPrestamo>();

            using var conn = _conexion.CrearConexion();
            await conn.OpenAsync();

            var query = @"SELECT id_historial, id_reserva, id_usuario, fecha_prestamo, fecha_devolucion, estado
                  FROM historial_prestamos
                  WHERE id_usuario = @id
                  ORDER BY fecha_prestamo DESC";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", idUsuario);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                historial.Add(new HistorialPrestamo
                {
                    IdHistorial = reader.GetInt64(0),
                    IdReserva = reader.GetInt64(1),
                    IdUsuario = reader.GetInt32(2),
                    FechaPrestamo = reader.GetDateTime(3),
                    FechaDevolucion = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Estado = reader.IsDBNull(5) ? "" : reader.GetString(5)
                });
            }

            return historial;
        }


    }
}
