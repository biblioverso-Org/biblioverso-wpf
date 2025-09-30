using BCrypt.Net;
using library.Data;
using library.Models;
using library.Services;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace library.Services
{
    public class AuthService : IAuthService
    {
        private readonly Conexion _conexion;
        private readonly HttpClient _http;
        private const string BaseUrl = "http://localhost:4000/api/v1";

        public AuthService(Conexion conexion)
        {
            _conexion = conexion;
            _http = new HttpClient();
        }

        public async Task<Usuario?> LoginAsync(string username, string password)
        {
            try
            {
                using var connection = _conexion.CrearConexion(); 
                await connection.OpenAsync(); // Buscar por username o email
                var query = @" SELECT u.id_usuario, u.cl_usuario, u.usuario, u.password, u.nombre, u.apellido, u.email, u.telefono, u.direccion, u.genero, u.fecha_nac, u.nacionalidad, u.biografia, u.foto, u.id_rol, u.fecha_creacion, u.fecha_actualizacion, r.nombre as rol_nombre, r.descripcion as rol_descripcion FROM usuario u LEFT JOIN rol r ON u.id_rol = r.id_rol WHERE u.usuario = @username OR u.email = @username";
                using var command = new NpgsqlCommand(query, connection); 
                command.Parameters.AddWithValue("@username", username); 
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync()) 
                { var usuario = MapReaderToUsuario(reader); 
                    var storedPassword = reader["password"]?.ToString();
                    if (!string.IsNullOrEmpty(storedPassword)) 
                    {  bool isValidPassword = false; try {  isValidPassword = BCrypt.Net.BCrypt.Verify(password, storedPassword); } 
                        catch { isValidPassword = password == storedPassword; } if (isValidPassword) { return usuario; } } } return null; } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error en LoginAsync: {ex.Message}"); throw new Exception($"Error al autenticar usuario: {ex.Message}", ex); } }
        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            try
            {
                using var connection = _conexion.CrearConexion();
                await connection.OpenAsync();

                var query = @"
                    SELECT u.id_usuario, u.cl_usuario, u.usuario, u.password, u.nombre, u.apellido, 
                           u.email, u.telefono, u.direccion, u.genero, u.fecha_nac, u.nacionalidad, 
                           u.biografia, u.foto, u.id_rol, u.fecha_creacion, u.fecha_actualizacion,
                           r.nombre as rol_nombre, r.descripcion as rol_descripcion
                    FROM usuario u
                    LEFT JOIN rol r ON u.id_rol = r.id_rol
                    WHERE u.id_usuario = @id";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapReaderToUsuario(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetUsuarioByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Usuario?> GetUsuarioByUsernameAsync(string username)
        {
            try
            {
                using var connection = _conexion.CrearConexion();
                await connection.OpenAsync();

                var query = @"
                    SELECT u.id_usuario, u.cl_usuario, u.usuario, u.password, u.nombre, u.apellido, 
                           u.email, u.telefono, u.direccion, u.genero, u.fecha_nac, u.nacionalidad, 
                           u.biografia, u.foto, u.id_rol, u.fecha_creacion, u.fecha_actualizacion,
                           r.nombre as rol_nombre, r.descripcion as rol_descripcion
                    FROM usuario u
                    LEFT JOIN rol r ON u.id_rol = r.id_rol
                    WHERE u.usuario = @username";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapReaderToUsuario(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetUsuarioByUsernameAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Usuario?> GetUsuarioByEmailAsync(string email)
        {
            try
            {
                using var connection = _conexion.CrearConexion();
                await connection.OpenAsync();

                var query = @"
                    SELECT u.id_usuario, u.cl_usuario, u.usuario, u.password, u.nombre, u.apellido, 
                           u.email, u.telefono, u.direccion, u.genero, u.fecha_nac, u.nacionalidad, 
                           u.biografia, u.foto, u.id_rol, u.fecha_creacion, u.fecha_actualizacion,
                           r.nombre as rol_nombre, r.descripcion as rol_descripcion
                    FROM usuario u
                    LEFT JOIN rol r ON u.id_rol = r.id_rol
                    WHERE u.email = @email";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapReaderToUsuario(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetUsuarioByEmailAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Usuario>> GetAllUsuariosAsync()
        {
            var usuarios = new List<Usuario>();

            try
            {
                using var connection = _conexion.CrearConexion();
                await connection.OpenAsync();

                var query = @"
                    SELECT u.id_usuario, u.cl_usuario, u.usuario, u.password, u.nombre, u.apellido, 
                           u.email, u.telefono, u.direccion, u.genero, u.fecha_nac, u.nacionalidad, 
                           u.biografia, u.foto, u.id_rol, u.fecha_creacion, u.fecha_actualizacion,
                           r.nombre as rol_nombre, r.descripcion as rol_descripcion
                    FROM usuario u
                    LEFT JOIN rol r ON u.id_rol = r.id_rol
                    ORDER BY u.nombre, u.apellido";

                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    usuarios.Add(MapReaderToUsuario(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetAllUsuariosAsync: {ex.Message}");
                throw;
            }

            return usuarios;
        }

        public async Task<List<Rol>> GetAllRolesAsync()
        {
            var roles = new List<Rol>();

            try
            {
                using var connection = _conexion.CrearConexion();
                await connection.OpenAsync();

                var query = @"
                    SELECT id_rol, nombre, descripcion, fecha_creacion, fecha_actualizacion
                    FROM rol
                    ORDER BY nombre";

                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    roles.Add(new Rol
                    {
                        IdRol = Convert.ToInt32(reader["id_rol"]),
                        Nombre = reader["nombre"]?.ToString() ?? "",
                        Descripcion = reader["descripcion"] == DBNull.Value ? null : reader["descripcion"]?.ToString(),
                        FechaCreacion = reader["fecha_creacion"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(reader["fecha_creacion"]),
                        FechaActualizacion = reader["fecha_actualizacion"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(reader["fecha_actualizacion"])
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetAllRolesAsync: {ex.Message}");
                throw;
            }

            return roles;
        }

        public async Task<bool> ValidatePasswordAsync(string password, string hashedPassword)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                catch
                {
                    // Fallback para contraseñas en texto plano
                    return password == hashedPassword;
                }
            });
        }

        public async Task<Usuario?> CreateUsuarioAsync(Usuario usuario)
        {
            try
            {
                using var connection = _conexion.CrearConexion();
                await connection.OpenAsync();

                // Hash de la contraseña
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.Password);

                var query = @"
                    INSERT INTO usuario (cl_usuario, usuario, password, nombre, apellido, email, 
                                        telefono, direccion, genero, fecha_nac, nacionalidad, 
                                        biografia, foto, id_rol)
                    VALUES (@cl_usuario, @usuario, @password, @nombre, @apellido, @email, 
                            @telefono, @direccion, @genero, @fecha_nac, @nacionalidad, 
                            @biografia, @foto, @id_rol)
                    RETURNING id_usuario";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@cl_usuario", (object?)usuario.ClUsuario ?? DBNull.Value);
                command.Parameters.AddWithValue("@usuario", usuario.UserName);
                command.Parameters.AddWithValue("@password", hashedPassword);
                command.Parameters.AddWithValue("@nombre", (object?)usuario.Nombre ?? DBNull.Value);
                command.Parameters.AddWithValue("@apellido", (object?)usuario.Apellido ?? DBNull.Value);
                command.Parameters.AddWithValue("@email", (object?)usuario.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@telefono", (object?)usuario.Telefono ?? DBNull.Value);
                command.Parameters.AddWithValue("@direccion", (object?)usuario.Direccion ?? DBNull.Value);
                command.Parameters.AddWithValue("@genero", (object?)usuario.Genero ?? DBNull.Value);
                command.Parameters.AddWithValue("@fecha_nac", (object?)usuario.FechaNacimiento ?? DBNull.Value);
                command.Parameters.AddWithValue("@nacionalidad", (object?)usuario.Nacionalidad ?? DBNull.Value);
                command.Parameters.AddWithValue("@biografia", (object?)usuario.Biografia ?? DBNull.Value);
                command.Parameters.AddWithValue("@foto", (object?)usuario.Foto ?? DBNull.Value);
                command.Parameters.AddWithValue("@id_rol", (object?)usuario.IdRol ?? DBNull.Value);

                var newId = await command.ExecuteScalarAsync();

                if (newId != null)
                {
                    usuario.IdUsuario = Convert.ToInt32(newId);
                    return await GetUsuarioByIdAsync(usuario.IdUsuario);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CreateUsuarioAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
        {
            try
            {
                using var connection = _conexion.CrearConexion();
                await connection.OpenAsync();

                var query = @"
                    UPDATE usuario 
                    SET cl_usuario = @cl_usuario, usuario = @usuario, nombre = @nombre, 
                        apellido = @apellido, email = @email, telefono = @telefono, 
                        direccion = @direccion, genero = @genero, fecha_nac = @fecha_nac, 
                        nacionalidad = @nacionalidad, biografia = @biografia, foto = @foto, 
                        id_rol = @id_rol, fecha_actualizacion = now()
                    WHERE id_usuario = @id_usuario";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id_usuario", usuario.IdUsuario);
                command.Parameters.AddWithValue("@cl_usuario", (object?)usuario.ClUsuario ?? DBNull.Value);
                command.Parameters.AddWithValue("@usuario", usuario.UserName);
                command.Parameters.AddWithValue("@nombre", (object?)usuario.Nombre ?? DBNull.Value);
                command.Parameters.AddWithValue("@apellido", (object?)usuario.Apellido ?? DBNull.Value);
                command.Parameters.AddWithValue("@email", (object?)usuario.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@telefono", (object?)usuario.Telefono ?? DBNull.Value);
                command.Parameters.AddWithValue("@direccion", (object?)usuario.Direccion ?? DBNull.Value);
                command.Parameters.AddWithValue("@genero", (object?)usuario.Genero ?? DBNull.Value);
                command.Parameters.AddWithValue("@fecha_nac", (object?)usuario.FechaNacimiento ?? DBNull.Value);
                command.Parameters.AddWithValue("@nacionalidad", (object?)usuario.Nacionalidad ?? DBNull.Value);
                command.Parameters.AddWithValue("@biografia", (object?)usuario.Biografia ?? DBNull.Value);
                command.Parameters.AddWithValue("@foto", (object?)usuario.Foto ?? DBNull.Value);
                command.Parameters.AddWithValue("@id_rol", (object?)usuario.IdRol ?? DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en UpdateUsuarioAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            try
            {
                using var connection = _conexion.CrearConexion();
                await connection.OpenAsync();

                var query = "DELETE FROM usuario WHERE id_usuario = @id";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en DeleteUsuarioAsync: {ex.Message}");
                throw;
            }
        }

        private Usuario MapReaderToUsuario(NpgsqlDataReader reader)
        {
            var usuario = new Usuario
            {
                IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                ClUsuario = reader["cl_usuario"] == DBNull.Value ? null : reader["cl_usuario"]?.ToString(),
                UserName = reader["usuario"]?.ToString() ?? "",
                Password = reader["password"]?.ToString() ?? "",
                Nombre = reader["nombre"] == DBNull.Value ? null : reader["nombre"]?.ToString(),
                Apellido = reader["apellido"] == DBNull.Value ? null : reader["apellido"]?.ToString(),
                Email = reader["email"] == DBNull.Value ? null : reader["email"]?.ToString(),
                Telefono = reader["telefono"] == DBNull.Value ? null : reader["telefono"]?.ToString(),
                Direccion = reader["direccion"] == DBNull.Value ? null : reader["direccion"]?.ToString(),
                Genero = reader["genero"] == DBNull.Value ? null : reader["genero"]?.ToString(),
                FechaNacimiento = reader["fecha_nac"] == DBNull.Value ? null : Convert.ToDateTime(reader["fecha_nac"]),
                Nacionalidad = reader["nacionalidad"] == DBNull.Value ? null : reader["nacionalidad"]?.ToString(),
                Biografia = reader["biografia"] == DBNull.Value ? null : reader["biografia"]?.ToString(),
                Foto = reader["foto"] == DBNull.Value ? null : reader["foto"]?.ToString(),
                IdRol = reader["id_rol"] == DBNull.Value ? null : Convert.ToInt32(reader["id_rol"]),
                FechaCreacion = reader["fecha_creacion"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["fecha_creacion"]),
                FechaActualizacion = reader["fecha_actualizacion"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["fecha_actualizacion"])
            };

            // Mapear el rol si está presente
            if (reader["rol_nombre"] != DBNull.Value)
            {
                usuario.Rol = new Rol
                {
                    IdRol = usuario.IdRol ?? 0,
                    Nombre = reader["rol_nombre"]?.ToString() ?? "",
                    Descripcion = reader["rol_descripcion"] == DBNull.Value
                        ? null
                        : reader["rol_descripcion"]?.ToString()
                };
            }

            return usuario;
        }
    }
}