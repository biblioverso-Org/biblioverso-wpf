// ViewModels/CustomersViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Abstractions;
using library.Dialogs;
using library.Models;
using library.Services;
using MahApps.Metro.IconPacks;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;

namespace library.ViewModels
{
    public partial class CustomersViewModel : ObservableObject, ISearchable
    {
        private readonly UsuarioService _service;

        public IAsyncRelayCommand AddCustomerAsyncCommand { get; }
        public IAsyncRelayCommand<Usuario?> EditCustomerCommand { get; }
        public IAsyncRelayCommand<Usuario?> DeleteCustomerCommand { get; }

        public ObservableCollection<Usuario> Customers { get; }
        public ICollectionView CustomersView { get; }

        public ObservableCollection<DashStat> Stats { get; } = new();

        private string _search = string.Empty;

        public CustomersViewModel()
        {
            _service = new UsuarioService();
            Customers = new ObservableCollection<Usuario>();

            AddCustomerAsyncCommand = new AsyncRelayCommand(AddCustomerAsync);
            EditCustomerCommand = new AsyncRelayCommand<Usuario?>(EditCustomerAsync);
            DeleteCustomerCommand = new AsyncRelayCommand<Usuario?>(DeleteCustomerAsync);

            CustomersView = CollectionViewSource.GetDefaultView(Customers);
            CustomersView.Filter = FilterCustomer;

            _ = LoadClientesAsync();
        }

        private async Task LoadClientesAsync()
        {
            Customers.Clear();
            var clientes = await _service.GetClientesAsync();
            foreach (var c in clientes) Customers.Add(c);

            UpdateStats();
        }

        // ===================== Agregar cliente =====================
        private async Task AddCustomerAsync()
        {
            var vm = new AddCustomerDialogViewModel();
            var view = new AddCustomerDialog { DataContext = vm };

            var result = await DialogHost.Show(view, "RootDialog");
            if (result is AddCustomerDialogViewModel m)
            {
                // Subir la imagen a Cloudinary
                var cloudService = new CloudinaryService(
                    "dvw5h3ccw",
                    "893598289963378",
                    "mKNQQGTlypYx947y0F72jpnzb88"
                );

                string? fotoUrl = null;
                if (!string.IsNullOrEmpty(m.Foto))
                {
                    fotoUrl = await cloudService.UploadImageAsync(m.Foto);
                }

                var nuevo = new Usuario
                {
                    UserName = m.UserName,
                    Password = m.Password,
                    Nombre = m.Nombre,
                    Apellido = m.Apellido,
                    Email = m.Email,
                    Telefono = m.Telefono,
                    Direccion = m.Direccion,
                    Genero = m.Genero.ToString(),
                    Foto = fotoUrl,
                    FechaNacimiento = m.FechaNacimiento,
                    Nacionalidad = m.Nacionalidad,
                    Biografia = m.Biografia
                };

                await _service.AddClienteAsync(nuevo);
                await LoadClientesAsync();
            }
        }

        // ===================== Editar cliente =====================
        private async Task EditCustomerAsync(Usuario? c)
        {
            if (c is null) return;

            var vm = EditCustomerDialogViewModel.FromUsuario(c);
            var view = new EditCustomerDialog { DataContext = vm };
            var result = await DialogHost.Show(view, "RootDialog");

            if (result is EditCustomerDialogViewModel m)
            {
                var cloudService = new CloudinaryService(
                    "dvw5h3ccw",
                    "893598289963378",
                    "mKNQQGTlypYx947y0F72jpnzb88"
                );

                string? fotoUrl = c.Foto;
                if (!string.IsNullOrEmpty(m.Foto))
                {
                    fotoUrl = await cloudService.UploadImageAsync(m.Foto);
                }

                c.Nombre = m.Nombre;
                c.Apellido = m.Apellido;
                c.Email = m.Email;
                c.Telefono = m.Telefono;
                c.Direccion = m.Direccion;
                c.UserName = m.UserName;
                c.Password = m.Password;
                c.FechaNacimiento = m.FechaNacimiento;
                c.Nacionalidad = m.Nacionalidad;
                c.Biografia = m.Biografia;
                c.Genero = m.Genero.ToString();
                c.Foto = fotoUrl;

                await _service.UpdateClienteAsync(c);
                await LoadClientesAsync();
            }
        }

        // ===================== Eliminar cliente =====================
        private async Task DeleteCustomerAsync(Usuario? c)
        {
            if (c is null) return;

            var vm = new ConfirmDeleteCustomerDialogViewModel(c);
            var view = new ConfirmDeleteCustomerDialog { DataContext = vm };

            var result = await DialogHost.Show(view, "RootDialog");

            if (result?.ToString() == "True")
            {
                await _service.DeleteClienteAsync(c.IdUsuario);
                await LoadClientesAsync();
            }

        }

        // ===================== Filtro búsqueda =====================
        private bool FilterCustomer(object obj)
        {
            if (string.IsNullOrWhiteSpace(_search)) return true;
            if (obj is not Usuario c) return false;

            return (c.Nombre ?? "").Contains(_search, System.StringComparison.OrdinalIgnoreCase) ||
                   (c.Apellido ?? "").Contains(_search, System.StringComparison.OrdinalIgnoreCase) ||
                   (c.Email ?? "").Contains(_search, System.StringComparison.OrdinalIgnoreCase) ||
                   (c.UserName ?? "").Contains(_search, System.StringComparison.OrdinalIgnoreCase);
        }

        public void SetSearch(string? text)
        {
            _search = text ?? string.Empty;
            CustomersView.Refresh();
        }

        // ===================== Estadísticas =====================
        private void UpdateStats()
        {
            Stats.Clear();

            int total = Customers.Count;
            int nuevos = Customers.Count(c => c.FechaCreacion.HasValue &&
                                               c.FechaCreacion.Value > DateTime.Now.AddDays(-30));
            int activos = Customers.Count(c => c.FechaActualizacion.HasValue &&
                                               c.FechaActualizacion.Value > DateTime.Now.AddDays(-15));

            int deltaTotal = 0;
            int deltaNuevos = nuevos;
            int deltaActivos = activos - (int)(total * 0.1);

            Stats.Add(new DashStat("Usuarios Totales", total, deltaTotal, PackIconMaterialKind.AccountGroup));
            Stats.Add(new DashStat("Usuarios Nuevos", nuevos, deltaNuevos, PackIconMaterialKind.AccountPlus));
            Stats.Add(new DashStat("Usuarios Activos", activos, deltaActivos, PackIconMaterialKind.AccountCheck));
        }
    }
}
