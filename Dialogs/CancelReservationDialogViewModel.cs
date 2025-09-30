using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;

namespace library.Dialogs
{
    public partial class CancelReservationDialogViewModel : ObservableObject
    {
        public Reserva Reserva { get; }
        [ObservableProperty] private string? reason;

        public CancelReservationDialogViewModel(Reserva r)
        {
            Reserva = r;
        }
    }
}
