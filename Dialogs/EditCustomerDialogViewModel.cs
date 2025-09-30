using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarfBuzzSharp;
using library.Models;
using Microsoft.Win32;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class EditCustomerDialogViewModel : ObservableObject
{
    [ObservableProperty] private string? foto;              // antes PhotoPath
    [ObservableProperty] private string? nombre = "";
    [ObservableProperty] private string? apellido = "";

    [ObservableProperty] private string? email = "";
    [ObservableProperty] private string? telefono = "";
    [ObservableProperty] private string? direccion = "";
    [ObservableProperty] private string userName = "";      // antes Username
    [ObservableProperty] private string password = "";
    [ObservableProperty] private DateTime? fechaNacimiento;
    [ObservableProperty] private string? nacionalidad = "";
    [ObservableProperty] private string? biografia = "";
    [ObservableProperty] private string? genero = "Otro";   // ahora string, no enum fijo

    // Lista de géneros predefinidos
    public IReadOnlyList<string> Genders { get; } = new List<string>
    {
        "Masculino", "Femenino", "Otro"
    };

    // Validación mínima
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Nombre)
        && !string.IsNullOrWhiteSpace(Apellido)
        && !string.IsNullOrWhiteSpace(Email);

    partial void OnNombreChanged(string? value) => OnPropertyChanged(nameof(IsValid));
    partial void OnApellidoChanged(string? value) => OnPropertyChanged(nameof(IsValid));
    partial void OnEmailChanged(string? value) => OnPropertyChanged(nameof(IsValid));

    // Selección de imagen
    [RelayCommand]
    private void BrowseImage()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.webp;*.bmp",
            Multiselect = false
        };
        if (dlg.ShowDialog() == true)
            Foto = dlg.FileName;
    }

  


    // Cargar datos desde Usuario
    public static EditCustomerDialogViewModel FromUsuario(Usuario u) => new()
    {
        Foto = u.Foto,
        Nombre = u.Nombre ?? "",
        Apellido = u.Apellido ?? "",
        Email = u.Email ?? "",
        Telefono = u.Telefono ?? "",
        Direccion = u.Direccion ?? "",
        UserName = u.UserName,
        Password = u.Password,
        FechaNacimiento = u.FechaNacimiento,
        Nacionalidad = u.Nacionalidad,
        Biografia = u.Biografia,
        Genero = string.IsNullOrWhiteSpace(u.Genero) ? "Otro" : u.Genero
    };

    // Mapear de vuelta al modelo Usuario
    public void UpdateUsuario(Usuario u)
    {
        u.Foto = Foto;
        u.Nombre = Nombre;
        u.Apellido = Apellido;
        u.Email = Email;
        u.Telefono = Telefono;
        u.Direccion = Direccion;
        u.UserName = UserName;
        u.Password = Password;
        u.FechaNacimiento = FechaNacimiento;
        u.Nacionalidad = Nacionalidad;
        u.Biografia = Biografia;
        u.Genero = Genero;
        u.FechaActualizacion = DateTime.Now;
    }
}
