using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using MaterialDesignThemes.Wpf;

namespace library.Dialogs
{
    public partial class LoanReturnDialogViewModel : ObservableObject
    {
        [ObservableProperty] private Prestamo prestamo;
        [ObservableProperty] private bool notificarUsuario;
        [ObservableProperty] private string? observaciones;
        [ObservableProperty] private bool devolucionParcial;
        [ObservableProperty] private int cantidadDevuelta;

        public LoanReturnDialogViewModel(Prestamo p)
        {
            Prestamo = p;
            NotificarUsuario = true;
            DevolucionParcial = false;
            CantidadDevuelta = 1; // por defecto una unidad
        }

        [RelayCommand]
        private void Confirmar()
        {
            // ✅ Devuelve el préstamo al ViewModel principal
            DialogHost.CloseDialogCommand.Execute(Prestamo, null);
        }

        [RelayCommand]
        private void Cancelar()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
