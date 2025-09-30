using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarfBuzzSharp;
using library.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace library.Dialogs;

public partial class AddCustomerDialogViewModel : ObservableObject
{
    [ObservableProperty] private string? foto;             // Foto de perfil
    [ObservableProperty] private string userName = "";     // Usuario (requerido)
    [ObservableProperty] private string password = "";     // Contraseña (requerido)
    [ObservableProperty] private string? nombre = "";
    [ObservableProperty] private string? apellido = "";
    [ObservableProperty] private string? email = "";
    [ObservableProperty] private string? telefono = "";
    [ObservableProperty] private string? direccion = "";
    [ObservableProperty] private string? genero = "";
    [ObservableProperty] private DateTime? fechaNacimiento;
    [ObservableProperty] private string? nacionalidad = "";
    [ObservableProperty] private string? biografia = "";

    // Lista de géneros predefinidos (puedes extenderla desde DB si lo prefieres)
    public IReadOnlyList<string> Genders { get; } = new List<string>
    {
        "Masculino", "Femenino", "Otro"
    };

    // Validación mínima
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(UserName)
        && !string.IsNullOrWhiteSpace(Password)
        && !string.IsNullOrWhiteSpace(Nombre)
        && !string.IsNullOrWhiteSpace(Apellido)
        && !string.IsNullOrWhiteSpace(Email);

    partial void OnUserNameChanged(string value) => OnPropertyChanged(nameof(IsValid));
    partial void OnPasswordChanged(string value) => OnPropertyChanged(nameof(IsValid));
    partial void OnNombreChanged(string? value) => OnPropertyChanged(nameof(IsValid));
    partial void OnApellidoChanged(string? value) => OnPropertyChanged(nameof(IsValid));
    partial void OnEmailChanged(string? value) => OnPropertyChanged(nameof(IsValid));

    // Abrir selector de imágenes
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

    // Método auxiliar: mapear a modelo Usuario
    public Usuario ToUsuario() => new Usuario
    {
        UserName = UserName,
        Password = Password,
        Nombre = Nombre,
        Apellido = Apellido,
        Email = Email,
        Telefono = Telefono,
        Direccion = Direccion,
        Genero = Genero,
        FechaNacimiento = FechaNacimiento,
        Nacionalidad = Nacionalidad,
        Biografia = Biografia,
        Foto = Foto,
        FechaCreacion = DateTime.Now,
        FechaActualizacion = DateTime.Now
    };
}
