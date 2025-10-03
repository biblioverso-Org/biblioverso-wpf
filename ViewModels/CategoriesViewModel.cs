using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Dialogs;
using library.Models;
using library.Services;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace library.ViewModels
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly CategoriaService _service;

        public ObservableCollection<Categoria> Categorias { get; } = new();

        public IAsyncRelayCommand AddCategoriaCommand { get; }
        public IAsyncRelayCommand<Categoria?> EditCategoriaCommand { get; }
        public IAsyncRelayCommand<Categoria?> DeleteCategoriaCommand { get; }

        public CategoriesViewModel()
        {
            _service = new CategoriaService();
            AddCategoriaCommand = new AsyncRelayCommand(AddCategoriaAsync);
            EditCategoriaCommand = new AsyncRelayCommand<Categoria?>(EditCategoriaAsync);
            DeleteCategoriaCommand = new AsyncRelayCommand<Categoria?>(DeleteCategoriaAsync);

            _ = LoadCategoriasAsync();
        }

        private async Task LoadCategoriasAsync()
        {
            Categorias.Clear();
            var data = await _service.GetCategoriasAsync();
            foreach (var c in data) Categorias.Add(c);
        }

        private async Task AddCategoriaAsync()
        {
            var vm = new AddCategoriaDialogViewModel();
            var view = new AddCategoriaDialog { DataContext = vm };
            var result = await DialogHost.Show(view, "RootDialog");

            if (result is AddCategoriaDialogViewModel m)
            {
                await _service.AddCategoriaAsync(new Categoria { Nombre = m.Nombre });
                await LoadCategoriasAsync();
            }
        }

        private async Task EditCategoriaAsync(Categoria? c)
        {
            if (c is null) return;
            var vm = new EditCategoriaDialogViewModel(c);
            var view = new EditCategoriaDialog { DataContext = vm };
            var result = await DialogHost.Show(view, "RootDialog");

            if (result is EditCategoriaDialogViewModel m)
            {
                c.Nombre = m.Nombre;
                await _service.UpdateCategoriaAsync(c);
                await LoadCategoriasAsync();
            }
        }

        private async Task DeleteCategoriaAsync(Categoria? c)
        {
            if (c is null) return;
            var vm = new ConfirmDeleteCategoriaDialogViewModel(c);
            var view = new ConfirmDeleteCategoriaDialog { DataContext = vm };
            var result = await DialogHost.Show(view, "RootDialog");

            if (result?.ToString() == "True")
            {
                await _service.DeleteCategoriaAsync(c.IdCategoria);
                await LoadCategoriasAsync();
            }
        }
    }
}
