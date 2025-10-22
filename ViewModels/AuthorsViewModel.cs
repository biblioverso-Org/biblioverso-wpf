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

            if (result is AuthorDialogViewModel m)
            {
                try
                {
                    var cloudService = new CloudinaryService(
                        "dvw5h3ccw",
                        "893598289963378",
                        "mKNQQGTlypYx947y0F72jpnzb88"
                    );

                    string? fotoUrl = autor.Foto;

                    // 📤 Subir nueva imagen si seleccionó una local
                    if (!string.IsNullOrEmpty(m.Foto) && !m.Foto.StartsWith("https://"))
                    {
                        fotoUrl = await cloudService.UploadImageAsync(m.Foto, "autores");
                    }

                    autor.Nombre = m.Nombre;
                    autor.Nacionalidad = m.Nacionalidad;
                    autor.FechaNac = m.FechaNac;
                    autor.FechaMuerte = m.FechaMuerte;
                    autor.Biografia = m.Biografia;
                    autor.Foto = fotoUrl;

                    await _service.UpdateAsync(autor);
                    await LoadAutoresAsync();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"⚠️ Error al actualizar autor:\n{ex.Message}");
                }
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
