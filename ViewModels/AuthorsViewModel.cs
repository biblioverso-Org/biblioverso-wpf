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
    public partial class AuthorsViewModel : ObservableObject
    {
        private readonly AutorService _service;

        [ObservableProperty] private ObservableCollection<Autor> autores = new();
        [ObservableProperty] private Autor? selectedAutor;

        // ✅ Solo comandos de edición y vista
        public IAsyncRelayCommand<Autor?> EditAutorCommand { get; }
        public IAsyncRelayCommand<Autor?> ViewInfoAutorCommand { get; }

        public AuthorsViewModel()
        {
            _service = new AutorService();

            EditAutorCommand = new AsyncRelayCommand<Autor?>(EditAutorAsync);
            ViewInfoAutorCommand = new AsyncRelayCommand<Autor?>(ViewInfoAutorAsync);

            _ = LoadAutoresAsync();
        }

        private async Task LoadAutoresAsync()
        {
            Autores.Clear();
            var data = await _service.GetAllAsync();
            foreach (var a in data) Autores.Add(a);
        }

        private async Task EditAutorAsync(Autor? autor)
        {
            if (autor is null) return;

            var vm = new AuthorDialogViewModel(autor);
            var view = new AuthorDialog { DataContext = vm };
            var result = await DialogHost.Show(view, "RootDialog");

            if (result is Autor updated)
            {
                await _service.UpdateAsync(updated);
                await LoadAutoresAsync();
            }
        }

        private async Task ViewInfoAutorAsync(Autor? autor)
        {
            if (autor is null) return;

            var vm = new AuthorInfoDialogViewModel(autor);
            var view = new AuthorInfoDialog { DataContext = vm };
            await DialogHost.Show(view, "RootDialog");
        }
    }
}
