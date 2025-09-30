using System.Windows.Controls;
using library.ViewModels;

namespace library.Controls
{
    public partial class OrdersTable : UserControl
    {
        public OrdersTable()
        {
            InitializeComponent();
            DataContext = new OrdersViewModel(); 
        }
    }
}
