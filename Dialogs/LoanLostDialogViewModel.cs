using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Models;
using MaterialDesignThemes.Wpf;

namespace library.Dialogs
{
    public partial class LoanLostDialogViewModel : ObservableObject
    {
        [ObservableProperty] private Prestamo prestamo;
        [ObservableProperty] private string? motivo;
        [ObservableProperty] private decimal monto;
        [ObservableProperty] private bool notificarUsuario;

        public LoanLostDialogViewModel(Prestamo p)
        {
            Prestamo = p;
            Monto = 0;
            NotificarUsuario = true;
        }

        [RelayCommand]
        private void Confirmar()
        {
            // ✅ Devuelve solo el préstamo como resultado
            DialogHost.CloseDialogCommand.Execute(Prestamo, null);
        }

        [RelayCommand]
        private void Cancelar()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
