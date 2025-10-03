using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;

namespace library.Dialogs
{
    public partial class CustomerInfoDialogViewModel : ObservableObject
    {
        public Usuario Cliente { get; }

        public CustomerInfoDialogViewModel(Usuario cliente)
        {
            Cliente = cliente;
        }
    }
}
