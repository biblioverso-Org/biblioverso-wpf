using library.Data;
using library.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace library.Services
{
    public class LibroService : ILibroService
    {
        private readonly Conexion _conexion;
        private readonly HttpClient _http = new();

        public LibroService()
        {
            _conexion = new Conexion();
        }

        private async Task<List<Autor>> GetAutoresPorLibroAsync(long idLibro)
        {
            var autores = new List<Autor>();

            var query = @"SELECT a.id_autor, a.nombre, a.nacionalidad, a.fecha_nac, a.fecha_muerte, a.biografia
                  FROM autor a
                  INNER JOIN libro_autor la ON la.id_autor = a.id_autor
                  WHERE la.id_libro = @idLibro";

            var parameters = new[] { new Npgsql.NpgsqlParameter("@idLibro", idLibro) };

            using var reader = await _conexion.ExecuteReaderAsync(query, parameters);
            while (await reader.ReadAsync())
            {
                autores.Add(new Autor
                {
                    IdAutor = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Nacionalidad = reader.IsDBNull(2) ? null : reader.GetString(2),
                    FechaNac = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    FechaMuerte = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Biografia = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return autores;
        }


        // ==================== LISTAR ====================
        public async Task<IEnumerable<Libro>> GetLibrosAsync()
        {
            var query = @"SELECT id_libro, isbn, titulo, portada, sinopsis, id_categoria, editorial, fecha_publicacion, eliminado
                  FROM libro 
                  WHERE eliminado = FALSE";
            var result = new List<Libro>();

            using var reader = await _conexion.ExecuteReaderAsync(query);
            while (await reader.ReadAsync())
            {
                var libro = new Libro
                {
                    IdLibro = reader.GetInt64(0),
                    ISBN = reader.GetString(1),
                    Titulo = reader.GetString(2),
                    Portada = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Sinopsis = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IdCategoria = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    Editorial = reader.IsDBNull(6) ? null : reader.GetString(6),
                    FechaPublicacion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    Eliminado = reader.GetBoolean(8)
                };

                // Autores
                libro.Autores = await GetAutoresPorLibroAsync(libro.IdLibro);

                // Stock
                libro.Stocks = await GetStockPorLibroAsync(libro.IdLibro);

                // Categoría
                if (libro.IdCategoria.HasValue)
                {
                    var catService = new CategoriaService();
                    var cats = await catService.GetCategoriasAsync();
                    libro.Categoria = cats.FirstOrDefault(c => c.IdCategoria == libro.IdCategoria.Value);
                }

                result.Add(libro);
            }

            return result;
        }



        public async Task<Libro?> GetLibroByIdAsync(long id)
        {
            var query = "SELECT * FROM libro WHERE id_libro=@id";
            var parameters = new[] { new NpgsqlParameter("@id", id) };
            using var reader = await _conexion.ExecuteReaderAsync(query, parameters);
            if (await reader.ReadAsync())
            {
                return new Libro
                {
                    IdLibro = reader.GetInt64(0),
                    ISBN = reader.GetString(1),
                    Titulo = reader.GetString(2)
                };
            }
            return null;
        }

        // ==================== INSERTAR SIMPLE ====================
        public async Task<int> AddLibroAsync(Libro libro)
        {
            var query = @"INSERT INTO libro (isbn, titulo, portada, sinopsis, id_categoria, editorial, fecha_publicacion) 
                  VALUES (@isbn, @titulo, @portada, @sinopsis, @idCategoria, @editorial, @fecha)";
            var parameters = new[]
            {
        new NpgsqlParameter("@isbn", libro.ISBN),
        new NpgsqlParameter("@titulo", libro.Titulo),
        new NpgsqlParameter("@portada", (object?)libro.Portada ?? DBNull.Value),
        new NpgsqlParameter("@sinopsis", (object?)libro.Sinopsis ?? DBNull.Value),
        new NpgsqlParameter("@idCategoria", (object?)libro.IdCategoria ?? DBNull.Value),
        new NpgsqlParameter("@editorial", (object?)libro.Editorial ?? DBNull.Value),
        new NpgsqlParameter("@fecha", (object?)libro.FechaPublicacion ?? DBNull.Value)
    };
            return await _conexion.ExecuteNonQueryAsync(query, parameters);
        }

        // ==================== INSERTAR COMPLETO (Libro + Autores + Stock) ====================
        public async Task<int> AddLibroCompletoAsync(Libro libro)
        {
            // 1. Insertar libro y obtener ID
            var queryLibro = @"INSERT INTO libro (isbn, titulo, portada, sinopsis, id_categoria, editorial, fecha_publicacion, fecha_creacion, fecha_actualizacion) 
                               VALUES (@isbn, @titulo, @portada, @sinopsis, @idCategoria, @editorial, @fecha_pub, @creacion, @actualizacion)
                               RETURNING id_libro";
            var parametersLibro = new[]
            {
                new NpgsqlParameter("@isbn", libro.ISBN),
                new NpgsqlParameter("@titulo", libro.Titulo),
                new NpgsqlParameter("@portada", (object?)libro.Portada ?? DBNull.Value),
                new NpgsqlParameter("@sinopsis", (object?)libro.Sinopsis ?? DBNull.Value),
                new NpgsqlParameter("@idCategoria", (object?)libro.IdCategoria ?? DBNull.Value),
                new NpgsqlParameter("@editorial", (object?)libro.Editorial ?? DBNull.Value),
                new NpgsqlParameter("@fecha_pub", (object?)libro.FechaPublicacion ?? DBNull.Value),
                new NpgsqlParameter("@creacion", DateTime.UtcNow),
                new NpgsqlParameter("@actualizacion", DateTime.UtcNow)
            };

            var idLibro = (long)(await _conexion.ExecuteScalarAsync(queryLibro, parametersLibro))!;
           
            // 2. Manejo de autores (evitar duplicados)
            foreach (var autor in libro.Autores)
            {
                int idAutor;

                // Buscar si ya existe
                var queryFind = "SELECT id_autor FROM autor WHERE nombre=@nombre LIMIT 1";
                var paramFind = new[] { new NpgsqlParameter("@nombre", autor.Nombre) };
                var idAutorObj = await _conexion.ExecuteScalarAsync(queryFind, paramFind);

                if (idAutorObj == null)
                {
                    var queryAutor = @"INSERT INTO autor (nombre, nacionalidad, fecha_nac, fecha_muerte, biografia, fecha_creacion, fecha_actualizacion) 
                                       VALUES (@nombre, @nac, @fnac, @fmuerte, @bio, @creacion, @actualizacion) RETURNING id_autor";
                    var paramAutor = new[]
                    {
                        new NpgsqlParameter("@nombre", autor.Nombre),
                        new NpgsqlParameter("@nac", (object?)autor.Nacionalidad ?? DBNull.Value),
                        new NpgsqlParameter("@fnac", (object?)autor.FechaNac ?? DBNull.Value),
                        new NpgsqlParameter("@fmuerte", (object?)autor.FechaMuerte ?? DBNull.Value),
                        new NpgsqlParameter("@bio", (object?)autor.Biografia ?? DBNull.Value),
                        new NpgsqlParameter("@creacion", DateTime.UtcNow),
                        new NpgsqlParameter("@actualizacion", DateTime.UtcNow)
                    };
                    idAutor = (int)(await _conexion.ExecuteScalarAsync(queryAutor, paramAutor))!;
                }
                else
                {
                    idAutor = (int)idAutorObj;
                }

                // Relación libro_autor
                var queryRel = "INSERT INTO libro_autor (id_libro, id_autor) VALUES (@idLibro, @idAutor)";
                var paramRel = new[]
                {
                    new NpgsqlParameter("@idLibro", idLibro),
                    new NpgsqlParameter("@idAutor", idAutor)
                };
                await _conexion.ExecuteNonQueryAsync(queryRel, paramRel);
            }

            // 3. Stock inicial
            foreach (var stock in libro.Stocks)
            {
                var queryStock = @"INSERT INTO stock (id_libro, ubicacion, disponibilidad, estado) 
                                   VALUES (@idLibro, @ubicacion, @disp, @estado)";
                var paramStock = new[]
                {
                    new NpgsqlParameter("@idLibro", idLibro),
                    new NpgsqlParameter("@ubicacion", (object?)stock.Ubicacion ?? "General"),
                    new NpgsqlParameter("@disp", stock.Disponibilidad),
                    new NpgsqlParameter("@estado", stock.Estado)
                };
                await _conexion.ExecuteNonQueryAsync(queryStock, paramStock);
            }

            return (int)idLibro;
        }

        // ==================== UPDATE ====================
        public async Task<int> UpdateLibroAsync(Libro libro)
        {
            var query = @"UPDATE libro 
                  SET titulo=@titulo, 
                      sinopsis=@sinopsis, 
                      id_categoria=@idCategoria, 
                      editorial=@editorial, 
                      portada=@portada, 
                      fecha_actualizacion=@fecha_actualizacion 
                  WHERE id_libro=@id";

            var parameters = new[]
            {
        new NpgsqlParameter("@titulo", libro.Titulo),
        new NpgsqlParameter("@sinopsis", (object?)libro.Sinopsis ?? DBNull.Value),
        new NpgsqlParameter("@idCategoria", (object?)libro.IdCategoria ?? DBNull.Value),
        new NpgsqlParameter("@editorial", (object?)libro.Editorial ?? DBNull.Value),
        new NpgsqlParameter("@portada", (object?)libro.Portada ?? DBNull.Value),
        new NpgsqlParameter("@fecha_actualizacion", DateTime.UtcNow),
        new NpgsqlParameter("@id", libro.IdLibro)
    };

            return await _conexion.ExecuteNonQueryAsync(query, parameters);
        }


        // ==================== DELETE ====================
        public async Task<int> DeleteLibroAsync(long id)
        {
            var query = @"UPDATE libro 
                  SET eliminado = TRUE, fecha_actualizacion = @fecha 
                  WHERE id_libro = @id";
            var parameters = new[]
            {
        new NpgsqlParameter("@fecha", DateTime.UtcNow),
        new NpgsqlParameter("@id", id)
    };
            return await _conexion.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task UpdateStockAsync(long idLibro, Dictionary<string, int> cambios)
        {
            foreach (var kv in cambios)
            {
                string estado = kv.Key;   // "Nuevo", "Usado", etc.
                int cantidad = kv.Value;  // puede ser + o -

                if (cantidad > 0)
                {
                    for (int i = 0; i < cantidad; i++)
                    {
                        var queryInsert = @"INSERT INTO stock (id_libro, ubicacion, disponibilidad, estado)
                                    VALUES (@idLibro, 'General', TRUE, @estado)
                                    RETURNING id_stock";
                        var paramInsert = new[]
                        {
                    new NpgsqlParameter("@idLibro", idLibro),
                    new NpgsqlParameter("@estado", estado)
                };

                        // 📌 insertamos y recuperamos id_stock
                        var idStock = (long)(await _conexion.ExecuteScalarAsync(queryInsert, paramInsert))!;

                        // 📌 Verificar lista de espera
                        var reservaService = new ReservaService();
                        var reserva = await reservaService.ObtenerPrimeraReservaPendiente(idLibro);

                        if (reserva != null)
                        {
                            // 1. Cambiar reserva a notificada
                            await reservaService.NotificarReservaAsync(reserva.IdReserva);

                            // 2. Crear notificación
                            var notifService = new NotificacionService();
                            await notifService.CrearNotificacionAsync(
                                reserva.IdUsuario,
                                $"📚 Libro disponible",
                                $"El libro que reservaste ya está disponible. Ven a recogerlo en las próximas 24h."
                            );

                            // 3. Marcar stock como ocupado temporalmente
                            var queryBloquear = "UPDATE stock SET disponibilidad=FALSE WHERE id_stock=@id";
                            await _conexion.ExecuteNonQueryAsync(queryBloquear, new[] {
                        new NpgsqlParameter("@id", idStock)
                    });
                        }
                    }
                }
                else if (cantidad < 0)
                {
                    int toRemove = -cantidad;

                    var queryDelete = @"DELETE FROM stock
                                WHERE id_stock IN (
                                    SELECT id_stock FROM stock
                                    WHERE id_libro = @idLibro
                                      AND estado = @estado
                                      AND disponibilidad = TRUE
                                    LIMIT @toRemove
                                )";

                    var paramDelete = new[]
                    {
                new NpgsqlParameter("@idLibro", idLibro),
                new NpgsqlParameter("@estado", estado),
                new NpgsqlParameter("@toRemove", toRemove)
            };

                    int removed = await _conexion.ExecuteNonQueryAsync(queryDelete, paramDelete);

                    if (removed < toRemove)
                    {
                        throw new InvalidOperationException(
                            $"No hay suficientes ejemplares disponibles para quitar {toRemove} en estado {estado}"
                        );
                    }
                }
            }
        }

        private async Task<List<Stock>> GetStockPorLibroAsync(long idLibro)
        {
            var stocks = new List<Stock>();

            var query = @"SELECT id_stock, id_libro, ubicacion, disponibilidad, estado
                  FROM stock
                  WHERE id_libro = @idLibro";

            var parameters = new[] { new NpgsqlParameter("@idLibro", idLibro) };

            using var reader = await _conexion.ExecuteReaderAsync(query, parameters);
            while (await reader.ReadAsync())
            {
                stocks.Add(new Stock
                {
                    IdStock = reader.GetInt64(0),
                    IdLibro = reader.GetInt64(1),
                    Ubicacion = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Disponibilidad = reader.GetBoolean(3),
                    Estado = reader.GetString(4)
                });
            }

            return stocks;
        }


        // ==================== GOOGLE BOOKS ====================
        public async Task<IEnumerable<Libro>> BuscarEnGoogleBooksAsync(string query)
        {
            var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}";
            var response = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var items = new List<Libro>();

            foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
            {
                var volume = item.GetProperty("volumeInfo");
                items.Add(new Libro
                {
                    ISBN = volume.TryGetProperty("industryIdentifiers", out var ids) && ids.GetArrayLength() > 0
                        ? ids[0].GetProperty("identifier").GetString() ?? ""
                        : "",
                    Titulo = volume.GetProperty("title").GetString() ?? "",
                    Autores = volume.TryGetProperty("authors", out var authors)
                        ? authors.EnumerateArray()
                            .Where(a => !string.IsNullOrWhiteSpace(a.GetString()))
                            .Select(a => new Autor { Nombre = a.GetString() ?? "" })
                            .ToList()
                        : new List<Autor>(),
                    Editorial = volume.TryGetProperty("publisher", out var pub) ? pub.GetString() : null,
                    FechaPublicacion = volume.TryGetProperty("publishedDate", out var date)
                        && DateTime.TryParse(date.GetString(), out var dt) ? dt : null,
                    Portada = volume.TryGetProperty("imageLinks", out var imgs)
                        && imgs.TryGetProperty("thumbnail", out var thumb) ? thumb.GetString() : null,
                    Sinopsis = volume.TryGetProperty("description", out var desc) ? desc.GetString() : null
                });
            }
            return items;
        }
    }


    public class CategoriaService
    {
        private readonly Conexion _conexion;
        public CategoriaService() => _conexion = new Conexion();

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            var result = new List<Categoria>();
            var query = "SELECT id_categoria, nombre FROM categoria ORDER BY nombre";

            using var reader = await _conexion.ExecuteReaderAsync(query);
            while (await reader.ReadAsync())
            {
                result.Add(new Categoria
                {
                    IdCategoria = reader.GetInt32(0),
                    Nombre = reader.GetString(1)
                });
            }

            return result;
        }


        public async Task<int> AddCategoriaAsync(Categoria c)
        {
            var query = "INSERT INTO categoria(nombre) VALUES(@nombre)";
            return await _conexion.ExecuteNonQueryAsync(query, new[] {
                new NpgsqlParameter("@nombre", c.Nombre)
            });
        }

        public async Task<int> UpdateCategoriaAsync(Categoria c)
        {
            var query = "UPDATE categoria SET nombre=@nombre WHERE id_categoria=@id";
            return await _conexion.ExecuteNonQueryAsync(query, new[] {
                new NpgsqlParameter("@nombre", c.Nombre),
                new NpgsqlParameter("@id", c.IdCategoria)
            });
        }

        public async Task<int> DeleteCategoriaAsync(int id)
        {
            var query = "DELETE FROM categoria WHERE id_categoria=@id";
            return await _conexion.ExecuteNonQueryAsync(query, new[] {
                new NpgsqlParameter("@id", id)
            });
        }
    }
}
