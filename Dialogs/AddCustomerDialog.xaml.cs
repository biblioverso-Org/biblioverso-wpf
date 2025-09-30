using System.Windows;
using System.Windows.Controls;

namespace library.Dialogs;

public partial class AddCustomerDialog : UserControl
{
    public AddCustomerDialog()
    {
        InitializeComponent();
    }


    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is AddCustomerDialogViewModel vm && sender is PasswordBox pb)
        {
            vm.Password = pb.Password;
        }
    }

}