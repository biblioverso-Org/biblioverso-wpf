using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;

namespace library.Dialogs;

public partial class ConfirmDeleteCustomerDialogViewModel : ObservableObject
{
    [ObservableProperty] private Usuario usuario;

    public ConfirmDeleteCustomerDialogViewModel(Usuario usuario)
    {
        Usuario = usuario;
    }

    public string FullName => $"{Usuario.Nombre} {Usuario.Apellido}".Trim();
    public string Email => Usuario.Email ?? "";
    public string? Foto => Usuario.Foto;
}
