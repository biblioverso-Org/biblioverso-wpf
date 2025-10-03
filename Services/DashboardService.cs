using library.Data;
using library.Models;
using MahApps.Metro.IconPacks;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace library.Services;

public class DashboardService
{
    private readonly Conexion _conexion = new Conexion();

    public async Task<List<DashStat>> GetStatsAsync()
    {
        var stats = new List<DashStat>();

        // 1. Libros en catálogo
        var libros = (long)(await _conexion.ExecuteScalarAsync("SELECT COUNT(*) FROM libro"))!;
        stats.Add(new DashStat("Libros en catálogo", (int)libros, 0, PackIconMaterialKind.BookMultiple));

        // 2. Préstamos activos
        var prestamos = (long)(await _conexion.ExecuteScalarAsync("SELECT COUNT(*) FROM prestamo WHERE estado='activo'"))!;
        stats.Add(new DashStat("Préstamos activos", (int)prestamos, 0, PackIconMaterialKind.BookOpenVariant));

        // 3. Reservas activas
        var reservas = (long)(await _conexion.ExecuteScalarAsync("SELECT COUNT(*) FROM reserva WHERE estado='activa'"))!;
        stats.Add(new DashStat("Reservas activas", (int)reservas, 0, PackIconMaterialKind.CalendarMonth));

        // 4. Usuarios activos (últimos 30 días)
        var usuariosActivos = (long)(await _conexion.ExecuteScalarAsync(
            "SELECT COUNT(DISTINCT id_usuario) FROM prestamo WHERE fecha_prestamo >= NOW() - INTERVAL '30 days'"))!;
        stats.Add(new DashStat("Usuarios activos (30d)", (int)usuariosActivos, 0, PackIconMaterialKind.AccountGroup));

        // 5. Nuevos títulos (este mes)
        var nuevosTitulos = (long)(await _conexion.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM libro WHERE DATE_TRUNC('month', fecha_creacion) = DATE_TRUNC('month', NOW())"))!;
        stats.Add(new DashStat("Nuevos títulos (mes)", (int)nuevosTitulos, 0, PackIconMaterialKind.BookPlus));

        // 6. Renovaciones (este mes)
        var renovaciones = (long)(await _conexion.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM prestamo WHERE renovado = TRUE AND DATE_TRUNC('month', fecha_actualizacion) = DATE_TRUNC('month', NOW())"))!;
        stats.Add(new DashStat("Renovaciones (mes)", (int)renovaciones, 0, PackIconMaterialKind.Autorenew));

        return stats;
    }

    // Ejemplo: Top 5 libros más reservados
    public async Task<List<(string titulo, int count)>> GetTopLibrosReservadosAsync()
    {
        var result = new List<(string, int)>();
        var query = @"
            SELECT l.titulo, COUNT(r.id_reserva) as total
            FROM reserva r
            JOIN libro l ON r.id_libro = l.id_libro
            GROUP BY l.titulo
            ORDER BY total DESC
            LIMIT 5";

        using var reader = await _conexion.ExecuteReaderAsync(query);
        while (await reader.ReadAsync())
        {
            result.Add((reader.GetString(0), reader.GetInt32(1)));
        }
        return result;
    }
}
