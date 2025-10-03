using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using library.Abstractions;
using library.Models;
using library.Services;
using library.ViewModels;
using MahApps.Metro.IconPacks;

public partial class ShellViewModel : ObservableObject
{
    private readonly IUserSessionService _userSessionService;

    [ObservableProperty] private object? currentViewModel;
    [ObservableProperty] private bool isSidebarExpanded = true;
    [ObservableProperty] private string? globalSearch;
    [ObservableProperty] private bool isDarkTheme;

    // Propiedades de autenticación
    [ObservableProperty] private bool isLoggedIn;
    [ObservableProperty] private Usuario? currentUser;
    [ObservableProperty] private string welcomeMessage = "Bienvenido";

    // ViewModel para el login
    [ObservableProperty] private DatabaseLoginViewModel loginViewModel;

    // Instancias únicas (ahora inyectadas por DI)
    public DashboardViewModel Dashboard { get; private set; }
    public CustomersViewModel Customers { get; private set; }
    public BooksViewModel Books { get; private set; }
    public MembersViewModel Members { get; private set; }
    public OrdersViewModel Orders { get; private set; }
    public TopBarViewModel TopBar { get; private set; }
    public LoanViewModel Loans { get; private set; }
    public CategoriesViewModel Categories { get; private set; }
    public AuthorsViewModel Authors { get; private set; }
    public HistorialViewModel Historial { get; private set; }



    public ObservableCollection<NavigationItem> NavItems { get; } = new();

    public ShellViewModel(
        IUserSessionService userSessionService,
        DatabaseLoginViewModel loginViewModel,
        DashboardViewModel dashboard,
        CustomersViewModel customers,
        BooksViewModel books,
        MembersViewModel members,
        LoanViewModel loans,
        OrdersViewModel orders,
        TopBarViewModel topBar,
        CategoriesViewModel categories,
        AuthorsViewModel authors,
        HistorialViewModel historial)
    {
        _userSessionService = userSessionService;
        LoginViewModel = loginViewModel;
        Dashboard = dashboard;
        Customers = customers;
        Books = books;
        Members = members;
        Loans = loans;
        Orders = orders;
        TopBar = topBar;
        Categories = categories;
        Authors = authors;
        Historial = historial;
        // Suscribirse a eventos del servicio de sesión
        _userSessionService.UserLoggedIn += OnUserLoggedIn;
        _userSessionService.UserLoggedOut += OnUserLoggedOut;

        // Suscribirse al evento de login exitoso del LoginViewModel
        LoginViewModel.LoginSuccess += OnLoginSuccess;

        SetupNavigation();
        UpdateAuthenticationState();

    }

    private void SetupNavigation()
    {
        NavItems.Clear();
        NavItems.Add(new NavigationItem("Expandir / Ocultar", PackIconMaterialKind.ChevronLeft, NavSection.Primary,
            isToggle: true));
        NavItems.Add(new NavigationItem("Dashboard", PackIconMaterialKind.ViewDashboardOutline, NavSection.Primary,
            active: true, targetVm: Dashboard));
        NavItems.Add(new NavigationItem("Clientes", PackIconMaterialKind.AccountGroupOutline, NavSection.Primary,
            targetVm: Customers));
        NavItems.Add(new NavigationItem("Libros", PackIconMaterialKind.BookMultipleOutline, NavSection.Primary,
            targetVm: Books));
        NavItems.Add(new NavigationItem("Prestamos", PackIconMaterialKind.AccountMultipleOutline, NavSection.Primary,
            targetVm: Loans));
        NavItems.Add(new NavigationItem("Reservas", PackIconMaterialKind.CalendarMonthOutline, NavSection.Primary,
            targetVm: Orders));
        NavItems.Add(new NavigationItem("Cerrar sesión", PackIconMaterialKind.LogoutVariant, NavSection.Secondary));
        NavItems.Add(new NavigationItem("Categorías", PackIconMaterialKind.LabelOutline, NavSection.Primary,
    targetVm: Categories));
        NavItems.Add(new NavigationItem("Autores", PackIconMaterialKind.AccountTieOutline, NavSection.Primary,
    targetVm: Authors));
        NavItems.Add(new NavigationItem("Historial", PackIconMaterialKind.History, NavSection.Primary,
    targetVm: Historial));
      


    }

    // Eventos de autenticación
    private void OnLoginSuccess(Usuario usuario)
    {
        _userSessionService.Login(usuario);
    }

    private void OnUserLoggedIn(Usuario usuario)
    {
        UpdateAuthenticationState();

        NavigateToDashboard();
    }

    private void OnUserLoggedOut()
    {
        UpdateAuthenticationState();
        CurrentViewModel = null;
    }

    private void UpdateAuthenticationState()
    {
        IsLoggedIn = _userSessionService.IsLoggedIn;
        CurrentUser = _userSessionService.CurrentUser;

        if (CurrentUser != null)
        {
            WelcomeMessage = $"Bienvenido, {CurrentUser.DisplayName}";
            TopBar.UpdateUserInfo(CurrentUser);

            // Asegura que siempre haya una vista visible tras el login
            if (CurrentViewModel == null)
            {
                NavigateToDashboard();
            }
        }
        else
        {
            WelcomeMessage = "Inicia sesión para continuar";
        }
    }

    // Funcionalidad existente
    partial void OnGlobalSearchChanged(string? value)
        => (CurrentViewModel as ISearchable)?.SetSearch(value);

    partial void OnCurrentViewModelChanged(object? value)
        => (value as ISearchable)?.SetSearch(GlobalSearch);

    [RelayCommand]
    public void ToggleSidebar() => IsSidebarExpanded = !IsSidebarExpanded;

    [RelayCommand]
    public void Select(NavigationItem item) => ExecuteNav(item);

    [RelayCommand]
    private void ExecuteNav(NavigationItem item)
    {
        if (!IsLoggedIn && !item.IsToggle) return; // Solo permitir toggle si no está logueado

        if (item.IsToggle)
        {
            ToggleSidebar();
            return;
        }

        foreach (var it in NavItems) it.IsActive = false;
        item.IsActive = true;

        if (item.TargetViewModel is not null)
        {
            CurrentViewModel = item.TargetViewModel;
            return;
        }

        // Acciones especiales (logout, etc.)
        if (item.Title.Contains("cerrar", StringComparison.OrdinalIgnoreCase))
        {
            Logout();
            return;
        }
    }

    // Navegación directa
    [RelayCommand]
    public void Navigate(object vm) => CurrentViewModel = vm;

    // Métodos de navegación específicos
    public void NavigateToDashboard()
    {
        if (!IsLoggedIn) return;
        CurrentViewModel = Dashboard;
    }

    public void NavigateToLoans()   // ✅ NUEVO
    {
        if (!IsLoggedIn) return;
        CurrentViewModel = Loans;
    }

    public void NavigateToAuthors()   // ✅ NUEVO
    {
        if (!IsLoggedIn) return;
        CurrentViewModel = Authors;
    }

    public void NavigateToHistorial()   // ✅ NUEVO
    {
        if (!IsLoggedIn) return;
        CurrentViewModel = Historial;
    }


    public void NavigateToCustomers()
    {
        if (!IsLoggedIn) return;
        CurrentViewModel = Customers;
    }

    public void NavigateToBooks()
    {
        if (!IsLoggedIn) return;
        CurrentViewModel = Books;
    }

   


    public void NavigateToCategories()
    {
        if (!IsLoggedIn) return;
        CurrentViewModel = Categories;
    }


    public void NavigateToOrders()
    {
        if (!IsLoggedIn) return;
        CurrentViewModel = Orders;
    }

    public void Logout()
    {
        _userSessionService.Logout();
    }

    // Cleanup
    ~ShellViewModel()
    {
        _userSessionService.UserLoggedIn -= OnUserLoggedIn;
        _userSessionService.UserLoggedOut -= OnUserLoggedOut;
        if (LoginViewModel != null)
        {
            LoginViewModel.LoginSuccess -= OnLoginSuccess;
        }
    }
}