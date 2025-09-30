using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json.Linq;

namespace library.Models
{
    public partial class DashStat : ObservableObject
    {
        [ObservableProperty] private string title;
        [ObservableProperty] private int value;
        [ObservableProperty] private int delta; // crecimiento/variación
        [ObservableProperty] private PackIconMaterialKind icon;

        public DashStat(string title, int value, int delta, PackIconMaterialKind icon)
        {
            Title = title;
            Value = value;
            Delta = delta;
            Icon = icon;
        }
    }
}
