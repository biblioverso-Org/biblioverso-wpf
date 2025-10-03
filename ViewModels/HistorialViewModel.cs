using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using library.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace library.ViewModels
{
    public partial class HistorialViewModel : ObservableObject
    {
        private readonly ActividadService _service;

        [ObservableProperty] private ObservableCollection<Actividad> actividades = new();
        [ObservableProperty] private string? filtroUsuario;
        [ObservableProperty] private string? filtroRol;
        [ObservableProperty] private DateTime? filtroDesde;
        [ObservableProperty] private DateTime? filtroHasta;

        public IAsyncRelayCommand LoadCommand { get; }
        public IAsyncRelayCommand FilterCommand { get; }

        public HistorialViewModel()
        {
            _service = new ActividadService();
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            FilterCommand = new AsyncRelayCommand(ApplyFiltersAsync);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            Actividades.Clear();
            var data = await _service.GetAllAsync();
            foreach (var act in data) Actividades.Add(act);
        }

        private async Task ApplyFiltersAsync()
        {
            Actividades.Clear();
            var data = await _service.GetFilteredAsync(FiltroUsuario, FiltroRol, FiltroDesde, FiltroHasta);
            foreach (var act in data) Actividades.Add(act);
        }
    }
}
