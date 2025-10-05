using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using library.Dialogs;
using library.Models;
using library.Services;
using MaterialDesignThemes.Wpf;

namespace library.ViewModels
{
    public partial class TopBarViewModel : ObservableObject
    {
        private readonly IUserSessionService? _userSessionService;
        [ObservableProperty] private string? searchText;
        [ObservableProperty] private int notificationsCount = 0;
        [ObservableProperty] private bool isDarkTheme;
        private readonly NotificacionService _notificacionService = new();
        [ObservableProperty] private string userName = "Usuario";
        [ObservableProperty] private string userRole = "Rol";
        [ObservableProperty] private string userInitials = "U";
        [ObservableProperty] private bool isAdmin = false;
        [ObservableProperty] private string sectionTitle = "Biblioverso";
        [ObservableProperty] private string sectionDescription = "Sistema de gestión bibliotecaria";
        public Usuario? CurrentUser { get; private set; }
        public IAsyncRelayCommand<Usuario?> EditUserCommand { get; }
        public bool HasNotifications => NotificationsCount > 0;
        partial void OnNotificationsCountChanged(int value) => OnPropertyChanged(nameof(HasNotifications));
        public TopBarViewModel()
        {
            EditUserCommand = new AsyncRelayCommand<Usuario?>(OpenEditProfileAsync);
            IsDarkTheme = ThemeService.IsDark;
        }
        public TopBarViewModel(IUserSessionService userSessionService) : this()
        {
            _userSessionService = userSessionService;
        }
        public void UpdateUserInfo(Usuario usuario)
        {
            CurrentUser = usuario;
            UserName = usuario.DisplayName;
            UserRole = usuario.RolDisplay;
            IsAdmin = usuario.EsAdministrador;
            var names = usuario.DisplayName.Split(' ');
            UserInitials = names.Length >= 2 ? $"{names[0][0]}{names[1][0]}".ToUpper() : usuario.DisplayName.Substring(0, Math.Min(2, usuario.DisplayName.Length)).ToUpper();
            _ = CargarNotificacionesCountAsync(usuario.IdUsuario);
        }
        public void UpdateSection(string title, string description)
        {
            SectionTitle = title;
            SectionDescription = description;
        }
        private async Task OpenEditProfileAsync(Usuario? usuario)
        {
            if (CurrentUser == null)
            {
                MessageBox.Show("⚠️ No hay usuario logueado para editar."); return;
            }
            var target = usuario ?? CurrentUser;
            var vm = EditProfileDialogViewModel.FromUsuario(target);
            var view = new EditProfileDialog { DataContext = vm };
            try
            {
                var result = await DialogHost.Show(view, "RootDialog");
                if (result is EditProfileDialogViewModel saved)
                {
                    var cloudService = new CloudinaryService("dvw5h3ccw", "893598289963378", "mKNQQGTlypYx947y0F72jpnzb88");
                    string? fotoUrl = target.Foto;
                    if (!string.IsNullOrEmpty(saved.Foto)) fotoUrl = await cloudService.UploadImageAsync(saved.Foto);
                    saved.ApplyToUsuario(CurrentUser);
                    CurrentUser.Foto = fotoUrl;
                    var userService = new UsuarioService();
                    await userService.UpdateUsuarioAsync(CurrentUser);
                    MessageBox.Show($"✅ Perfil actualizado:\n{CurrentUser.Nombre} {CurrentUser.Apellido}\n📧 {CurrentUser.Email}", "Perfil actualizado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show($"🔥 Error en DialogHost.Show: {ex.Message}"); }
        }
        public async Task CargarNotificacionesCountAsync(int userId)
        {
            var notifs = await _notificacionService.ObtenerNotificacionesAsync(userId, 50);
            NotificationsCount = notifs.Count(n => !n.Leida);
        }

       
        [RelayCommand] private void ToggleTheme() { ThemeService.Toggle(); IsDarkTheme = ThemeService.IsDark; }
        [RelayCommand] private void ClearSearch() { var shell = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.DataContext as ShellViewModel; if (shell != null) shell.GlobalSearch = string.Empty; }
        [RelayCommand] private void ShowProfile() { var userId = _userSessionService?.GetCurrentUserId(); MessageBox.Show($"📋 Perfil de usuario\n\n👤 Nombre: {UserName}\n🏷️ Rol: {UserRole}\n🆔 ID: {userId}\n🔑 Es Admin: {(IsAdmin ? "Sí" : "No")}\n📧 Email: {CurrentUser?.Email ?? "No especificado"}", "Mi Perfil - Biblioverso", MessageBoxButton.OK, MessageBoxImage.Information); }
        [RelayCommand] private void Logout() { var result = MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?", "Cerrar Sesión - Biblioverso", MessageBoxButton.YesNo, MessageBoxImage.Question); if (result == MessageBoxResult.Yes) { _userSessionService?.Logout(); } }
        [RelayCommand] private void ChangeLanguage(string lang) { }
        [RelayCommand] private async Task OpenNotifications() { var vm = new NotificationsViewModel(); if (CurrentUser != null) await vm.CargarNotificacionesAsync(CurrentUser.IdUsuario); var dialog = new NotificationsDialog { DataContext = vm }; await DialogHost.Show(dialog, "RootDialog"); UpdateNotificationCount(); }
        public async void UpdateNotificationCount() { if (CurrentUser == null) return; var service = new NotificacionService(); NotificationsCount = await service.ContarNoLeidasAsync(CurrentUser.IdUsuario); }
    }
}
