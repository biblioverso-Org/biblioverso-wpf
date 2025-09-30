using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace library.Dialogs
{
    public partial class ManageStockDialogViewModel : ObservableObject
    {
        private readonly ILibroService _libroService;

        public long IdLibro { get; }

        // 📌 Cantidades actuales
        public int StockActualNuevo { get; }
        public int StockActualUsado { get; }
        public int StockActualWorn { get; }
        public int StockActualDamaged { get; }
        public int StockActualRepaired { get; }
        public int StockActualRestoring { get; }

        // 📌 Ajustes (+/-)
        [ObservableProperty] private int adjNuevo;
        [ObservableProperty] private int adjUsado;
        [ObservableProperty] private int adjWorn;
        [ObservableProperty] private int adjDamaged;
        [ObservableProperty] private int adjRepaired;
        [ObservableProperty] private int adjRestoring;

        // 📌 Mensajes descriptivos
        public string MsgNuevo => GetMsg(AdjNuevo);
        public string MsgUsado => GetMsg(AdjUsado);
        public string MsgWorn => GetMsg(AdjWorn);
        public string MsgDamaged => GetMsg(AdjDamaged);
        public string MsgRepaired => GetMsg(AdjRepaired);
        public string MsgRestoring => GetMsg(AdjRestoring);

        // 📌 Totales por estado
        public int NuevoFinal => StockActualNuevo + AdjNuevo;
        public int UsadoFinal => StockActualUsado + AdjUsado;
        public int WornFinal => StockActualWorn + AdjWorn;
        public int DamagedFinal => StockActualDamaged + AdjDamaged;
        public int RepairedFinal => StockActualRepaired + AdjRepaired;
        public int RestoringFinal => StockActualRestoring + AdjRestoring;

        // 📌 Totales globales
        public int StockActualTotal =>
            StockActualNuevo + StockActualUsado + StockActualWorn +
            StockActualDamaged + StockActualRepaired + StockActualRestoring;

        public int StockFinalTotal =>
            NuevoFinal + UsadoFinal + WornFinal + DamagedFinal + RepairedFinal + RestoringFinal;

        public int Diferencia => StockFinalTotal - StockActualTotal;

        // 📌 Validación
        public bool CanSave => StockFinalTotal >= 0;

        public ManageStockDialogViewModel(Libro libro, ILibroService libroService)
        {
            _libroService = libroService;
            IdLibro = libro.IdLibro;

            // Contar stocks actuales por estado (normalizando)
            StockActualNuevo = libro.Stocks.Count(s => NormalizeEstado(s.Estado) == "Nuevo");
            StockActualUsado = libro.Stocks.Count(s => NormalizeEstado(s.Estado) == "Usado");
            StockActualWorn = libro.Stocks.Count(s => NormalizeEstado(s.Estado) == "Desgastado");
            StockActualDamaged = libro.Stocks.Count(s => NormalizeEstado(s.Estado) == "Dañado");
            StockActualRepaired = libro.Stocks.Count(s => NormalizeEstado(s.Estado) == "Reparado");
            StockActualRestoring = libro.Stocks.Count(s => NormalizeEstado(s.Estado) == "En restauración");

            // Inicializar ajustes en 0
            AdjNuevo = AdjUsado = AdjWorn = AdjDamaged = AdjRepaired = AdjRestoring = 0;

            // Suscribirse a cambios para refrescar mensajes dinámicos
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName?.StartsWith("Adj") == true)
                {
                    OnPropertyChanged(nameof(MsgNuevo));
                    OnPropertyChanged(nameof(MsgUsado));
                    OnPropertyChanged(nameof(MsgWorn));
                    OnPropertyChanged(nameof(MsgDamaged));
                    OnPropertyChanged(nameof(MsgRepaired));
                    OnPropertyChanged(nameof(MsgRestoring));
                    OnPropertyChanged(nameof(NuevoFinal));
                    OnPropertyChanged(nameof(UsadoFinal));
                    OnPropertyChanged(nameof(WornFinal));
                    OnPropertyChanged(nameof(DamagedFinal));
                    OnPropertyChanged(nameof(RepairedFinal));
                    OnPropertyChanged(nameof(RestoringFinal));
                    OnPropertyChanged(nameof(StockFinalTotal));
                    OnPropertyChanged(nameof(Diferencia));
                }
            };
        }

        // 📌 Guardar cambios
        [RelayCommand]
        public async Task GuardarCambiosAsync()
        {
            var cambios = new Dictionary<string, int>
            {
                { "Nuevo", AdjNuevo },
                { "Usado", AdjUsado },
                { "Desgastado", AdjWorn },
                { "Dañado", AdjDamaged },
                { "Reparado", AdjRepaired },
                { "En restauración", AdjRestoring }
            };

            await _libroService.UpdateStockAsync(IdLibro, cambios);
        }

        // 📌 Normalizador
        private static string NormalizeEstado(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) return "";
            var e = estado.Trim().ToLowerInvariant();
            return e switch
            {
                "nuevo" or "new" => "Nuevo",
                "usado" or "used" => "Usado",
                "desgastado" or "worn" => "Desgastado",
                "dañado" or "damaged" => "Dañado",
                "reparado" or "repaired" => "Reparado",
                "en restauración" or "restoring" => "En restauración",
                _ => estado
            };
        }

        // 📌 Generador de mensajes
        private static string GetMsg(int value)
        {
            if (value > 0) return $"➕ Agregando {value}";
            if (value < 0) return $"➖ Quitando {-value}";
            return "Sin cambios";
        }
    }
}
