using System;
using CommunityToolkit.Mvvm.ComponentModel;
using library.Models;

namespace library.Dialogs
{
    public partial class LoanReservationDialogViewModel : ObservableObject
    {
        public Reserva Reserva { get; }

        [ObservableProperty] private DateTime loanStart;
        [ObservableProperty] private DateTime dueDate;
        [ObservableProperty] private string? notes;
        [ObservableProperty] private bool resetFine = true;

        public bool IsValid => DueDate.Date >= LoanStart.Date;

        public LoanReservationDialogViewModel(Reserva r)
        {
            Reserva = r;
            LoanStart = DateTime.Today;

            // Fecha mínima de devolución sugerida (7 días)
            var minDue = LoanStart.AddDays(7).Date;
            DueDate = minDue;
        }
    }
}
