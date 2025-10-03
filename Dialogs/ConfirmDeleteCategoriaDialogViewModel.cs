using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;

namespace library.Dialogs
{
    public partial class ConfirmDeleteCategoriaDialogViewModel : ObservableObject
    {
        public int IdCategoria { get; }
        public string Nombre { get; }

        public ConfirmDeleteCategoriaDialogViewModel(Categoria categoria)
        {
            IdCategoria = categoria.IdCategoria;
            Nombre = categoria.Nombre;
        }
    }
}
