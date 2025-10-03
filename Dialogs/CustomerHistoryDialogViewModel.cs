using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;
using library.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace library.Dialogs
{
    public partial class CustomerHistoryDialogViewModel : ObservableObject
    {
        private readonly UsuarioService _service;
        public Usuario Cliente { get; }
        public ObservableCollection<HistorialPrestamo> Historial { get; } = new();

        public CustomerHistoryDialogViewModel(Usuario cliente, UsuarioService service)
        {
            Cliente = cliente;
            _service = service;

            _ = LoadHistorialAsync();
        }

        private async Task LoadHistorialAsync()
        {
            Historial.Clear();
            var data = await _service.GetHistorialByUsuarioAsync(Cliente.IdUsuario);
            foreach (var h in data) Historial.Add(h);
        }
    }
}
