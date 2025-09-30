using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarfBuzzSharp;
using library.Models;
using Microsoft.Win32;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace library.Dialogs;

public partial class EditProfileDialogViewModel : ObservableObject
{
    // Datos básicos
    [ObservableProperty] private string? nombre;
    [ObservableProperty] private string? apellido;
    [ObservableProperty] private string? email;
    [ObservableProperty] private string? userName;
    [ObservableProperty] private string? telefono;
    [ObservableProperty] private string? direccion;
    [ObservableProperty] private string? biografia;

    // Preferencias
    [ObservableProperty] private bool isDarkTheme;
    [ObservableProperty] private bool emailNotifications;

    // Avatar
    [ObservableProperty] private string? foto;

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Nombre) &&
        !string.IsNullOrWhiteSpace(Apellido) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(UserName);

    // Actualizar validaciones
    partial void OnNombreChanged(string? _) { OnPropertyChanged(nameof(IsValid)); OnPropertyChanged(nameof(NombreCompleto)); }
    partial void OnApellidoChanged(string? _) { OnPropertyChanged(nameof(IsValid)); OnPropertyChanged(nameof(NombreCompleto)); }
    partial void OnEmailChanged(string? _) => OnPropertyChanged(nameof(IsValid));
    partial void OnUserNameChanged(string? _) => OnPropertyChanged(nameof(IsValid));

    [RelayCommand]
    private void BrowseImage()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.webp;*.bmp",
            CheckFileExists = true
        };
        if (dlg.ShowDialog() == true && File.Exists(dlg.FileName))
        {
            Foto = dlg.FileName;
        }
    }

    [RelayCommand]
    private void RemoveImage() => Foto = null;

    // ===================== Helpers para mapear con Usuario =====================
    public static EditProfileDialogViewModel FromUsuario(Usuario u) => new()
    {
        Nombre = u.Nombre,
        Apellido = u.Apellido,
        Email = u.Email,
        UserName = u.UserName,
        Telefono = u.Telefono,
        Direccion = u.Direccion,
        Biografia = u.Biografia,
        Foto = u.Foto
    };

    public void ApplyToUsuario(Usuario u)
    {
        u.Nombre = Nombre;
        u.Apellido = Apellido;
        u.Email = Email;
        u.UserName = UserName;
        u.Telefono = Telefono;
        u.Direccion = Direccion;
        u.Biografia = Biografia;
        u.Foto = Foto;
    }
}
