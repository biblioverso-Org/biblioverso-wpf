using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;

namespace library.ViewModels
{
    public partial class AuthorInfoDialogViewModel : ObservableObject
    {
        [ObservableProperty] private Autor autor;

        public AuthorInfoDialogViewModel(Autor autor)
        {
            Autor = autor;
        }
    }
}
