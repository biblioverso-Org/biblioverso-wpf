using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using library.Dialogs.AddBook;

namespace library.Services
{
    public class GoogleBooksService
    {
        private readonly HttpClient _http;
        private CancellationTokenSource? _cts;

        public GoogleBooksService(HttpClient? httpClient = null)
        {
            _http = httpClient ?? new HttpClient();
        }

        public async Task<IEnumerable<BookPick>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<BookPick>();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&maxResults=10";
                using var response = await _http.GetAsync(url, _cts.Token);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(_cts.Token);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var results = new List<BookPick>();

                if (root.TryGetProperty("items", out var items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        var volume = item.GetProperty("volumeInfo");

                        var titulo = volume.GetPropertyOrNull("title") ?? "Sin título";
                        var authors = volume.GetPropertyArrayOrNull("authors");
                        var autoresTexto = authors.Length > 0 ? string.Join(", ", authors) : "Desconocido";
                        var isbn = volume.GetISBN();
                        var year = volume.GetPropertyOrNull("publishedDate")?.Split('-')[0];
                        var rating = volume.TryGetProperty("averageRating", out var rt) ? rt.GetDouble() : 0.0;
                        var portada = volume.TryGetProperty("imageLinks", out var links) &&
                                      links.TryGetProperty("thumbnail", out var thumb)
                                      ? thumb.GetString()
                                      : null;
                        var sinopsis = volume.GetPropertyOrNull("description") ?? "";
                        var editorial = volume.GetPropertyOrNull("publisher");
                        var categorias = volume.GetPropertyArrayOrNull("categories");
                        var genero = categorias.Length > 0 ? string.Join(", ", categorias) : "Sin categoría";

                        results.Add(new BookPick(
                            titulo,
                            autoresTexto,
                            isbn,
                            genero,
                            editorial,
                            int.TryParse(year, out var y) ? new DateTime(y, 1, 1) : (DateTime?)null,
                            rating,
                            portada,
                            sinopsis
                        ));
                    }
                }

                return results;
            }
            catch
            {
                return Array.Empty<BookPick>();
            }
        }
    }

    internal static class JsonExtensions
    {
        public static string? GetPropertyOrNull(this JsonElement e, string name)
            => e.TryGetProperty(name, out var v) ? v.GetString() : null;

        public static string[] GetPropertyArrayOrNull(this JsonElement e, string name)
        {
            if (e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Array)
            {
                var list = new List<string>();
                foreach (var x in v.EnumerateArray())
                    list.Add(x.GetString() ?? "");
                return list.ToArray();
            }
            return Array.Empty<string>();
        }

        public static string GetISBN(this JsonElement e)
        {
            if (e.TryGetProperty("industryIdentifiers", out var ids))
            {
                foreach (var id in ids.EnumerateArray())
                {
                    if (id.TryGetProperty("identifier", out var val))
                        return val.GetString() ?? "";
                }
            }
            return "";
        }
    }
}
