using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace library.Dialogs
{
    public partial class EditCategoriaDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private int idCategoria;

        [ObservableProperty]
        private string nombre = string.Empty;

        public EditCategoriaDialogViewModel(Categoria categoria)
        {
            IdCategoria = categoria.IdCategoria;
            Nombre = categoria.Nombre;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Nombre);
    }
}
