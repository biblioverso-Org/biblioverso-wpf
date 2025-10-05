using System.Collections.ObjectModel;
using System.Linq; // Max(), Sum(), Select(), Repeat()
using CommunityToolkit.Mvvm.ComponentModel;
using library.Charts;
using library.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MahApps.Metro.IconPacks;
using SkiaSharp;

namespace library.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public ObservableCollection<DashStat> Stats { get; } = new()
    {
        new DashStat("Libros en catálogo",      12873, 120, PackIconMaterialKind.BookMultiple),
        new DashStat("Préstamos activos",         342,  18, PackIconMaterialKind.BookOpenVariant),
        new DashStat("Reservas activas",           97,  -5, PackIconMaterialKind.CalendarMonth),
        new DashStat("Usuarios activos (30d)",    612,  42, PackIconMaterialKind.AccountGroup),
        new DashStat("Nuevos títulos (mes)",       64,   8, PackIconMaterialKind.BookPlus),
    };



    // Colección de definiciones de charts que consumirá ChartSummary
    public ObservableCollection<ChartDef> Charts { get; }

    public DashboardViewModel()
    {
        Charts = new ObservableCollection<ChartDef>();

        // --- datos demo ---
        var months = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
        var loans = new[] { 8, 10, 12, 18, 20, 26, 22, 19, 17, 21, 24, 28 };
        var reservas = new[] { 3, 10, 5, 3, 7, 3, 8, 6, 4, 9, 5, 7 };

        // ========= 1) Barras con fondo: Reservas mensuales =========
        var cap = NiceCeil(reservas.Max() * 1.0);
        var background = Enumerable.Repeat((double)cap, months.Length).ToArray();
        var values = reservas.Select(v => (double)v).ToArray();

        var reservasBars = new CartesianChartDef
        {
            Title = "Reservas mensuales",
            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    IsHoverable = false,
                    Values = background,
                    Stroke = null,
                    Fill = new SolidColorPaint(new SKColor(30, 30, 30, 30)),
                    IgnoresBarPosition = true
                },
                new ColumnSeries<double>
                {
                    Values = values,
                    Stroke = null,
                    Fill = new SolidColorPaint(new SKColor(0x5B,0x8D,0xEF)),
                    IgnoresBarPosition = true
                }
            },
            XAxes = new[]
            {
                new Axis
                {
                    Labels = months,
                    LabelsPaint     = new SolidColorPaint(new SKColor(0x88,0x88,0x88)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(0xEE,0xEE,0xEE)) { StrokeThickness = 1 }
                }
            },
            YAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = cap,
                    LabelsPaint     = new SolidColorPaint(new SKColor(0x88,0x88,0x88)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(0xEE,0xEE,0xEE)) { StrokeThickness = 1 }
                }
            }
        };

        // ========= 2) Línea: Préstamos por mes =========
        var maxLoans = loans.Max();
        var yTop = NiceCeil(maxLoans * 1.2);

        var loansLine = new CartesianChartDef
        {
            Title = "Préstamos por mes",
            Series = new ISeries[]
            {
                new LineSeries<int>
                {
                    Values = loans,
                    LineSmoothness = 0,
                    GeometrySize = 6,
                    Stroke = new SolidColorPaint(new SKColor(0x5B,0x8D,0xEF)) { StrokeThickness = 3 },
                    Fill = null
                }
            },
            XAxes = new[]
            {
                new Axis
                {
                    Labels = months,
                    LabelsPaint     = new SolidColorPaint(new SKColor(0x88,0x88,0x88)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(0xEE,0xEE,0xEE)) { StrokeThickness = 1 }
                }
            },
            YAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = yTop,
                    LabelsPaint     = new SolidColorPaint(new SKColor(0x88,0x88,0x88)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(0xEE,0xEE,0xEE)) { StrokeThickness = 1 }
                }
            }
        };

        // ========= 3) Barras: Libros por género =========
        var genres = new[] { "Ficción", "Ciencia", "Historia", "Infantil", "Tecnología" };
        var booksByGenre = new[] { 320, 140, 210, 180, 90 };

        var booksByGenreChart = new CartesianChartDef
        {
            Title = "Libros por género",
            Series = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = booksByGenre,
                    Stroke = new SolidColorPaint(new SKColor(0x5B,0x8D,0xEF)) { StrokeThickness = 2 },
                    Fill   = new SolidColorPaint(new SKColor(0xE6,0xEE,0xFF))
                }
            },
            XAxes = new[]
            {
                new Axis
                {
                    Labels = genres,
                    LabelsPaint     = new SolidColorPaint(new SKColor(0x88,0x88,0x88)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(0xEE,0xEE,0xEE)) { StrokeThickness = 1 }
                }
            },
            YAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0,
                    LabelsPaint     = new SolidColorPaint(new SKColor(0x88,0x88,0x88)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(0xEE,0xEE,0xEE)) { StrokeThickness = 1 }
                }
            }
        };

        // ========= 4) Donut: Distribución por género =========
        var total = booksByGenre.Sum();
        var dist = booksByGenre.Select(v => 100.0 * v / total).ToArray();

        var genreDistribution = new PieChartDef
        {
            Title = "Distribución por género",
            Series = new ISeries[]
            {
                new PieSeries<double> { Name = "Ficción",    Values = new[] { dist[0] }, InnerRadius = 40, Fill = new SolidColorPaint(new SKColor(0x8B,0xA7,0xFF)) },
                new PieSeries<double> { Name = "Ciencia",    Values = new[] { dist[1] }, InnerRadius = 40, Fill = new SolidColorPaint(new SKColor(0x5B,0x8D,0xEF)) },
                new PieSeries<double> { Name = "Historia",   Values = new[] { dist[2] }, InnerRadius = 40, Fill = new SolidColorPaint(new SKColor(0x21,0xC0,0x8B)) },
                new PieSeries<double> { Name = "Infantil",   Values = new[] { dist[3] }, InnerRadius = 40, Fill = new SolidColorPaint(new SKColor(0xF2,0xC9,0x1F)) },
                new PieSeries<double> { Name = "Tecnología", Values = new[] { dist[4] }, InnerRadius = 40, Fill = new SolidColorPaint(new SKColor(0xFB,0x6F,0x92)) }
            }
        };

        // Orden final en el dashboard
        Charts.Add(reservasBars);
        Charts.Add(loansLine);
        Charts.Add(booksByGenreChart);
        Charts.Add(genreDistribution);
    }

    private static double NiceCeil(double value)
    {
        if (value <= 10) return 10;
        var pow = System.Math.Pow(10, System.Math.Floor(System.Math.Log10(value)));
        var scaled = value / pow;
        var step = scaled <= 1 ? 1 : scaled <= 2 ? 2 : scaled <= 5 ? 5 : 10;
        return step * pow;
    }
}

// Otros VMs (placeholders)
public partial class BooksViewModel : ObservableObject { }
public partial class MembersViewModel : ObservableObject { }
public partial class OrdersViewModel : ObservableObject { }
public partial class SettingsViewModel : ObservableObject { }