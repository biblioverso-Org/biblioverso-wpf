using CommunityToolkit.Mvvm.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace library.Dialogs
{
    public partial class AddCategoriaDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string nombre = string.Empty;

        public bool IsValid => !string.IsNullOrWhiteSpace(Nombre);
    }
}
