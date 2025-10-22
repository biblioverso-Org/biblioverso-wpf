using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;
using System.Windows.Forms;

namespace library.Dialogs
{
    public partial class MarkAsReadyDialogViewModel : ObservableObject
    {
        public Reserva Reserva { get; }

        [ObservableProperty] private string? mensaje;

        public MarkAsReadyDialogViewModel(Reserva r)
        {
            Reserva = r;
            Mensaje = $"El libro \"{r.Libro?.Titulo}\" está listo para ser recogido por {r.Usuario?.NombreCompleto}.";
        }
    }
}
