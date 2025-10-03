using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using library.Data;
using library.Services;
using library.ViewModels;

namespace library
{
    public partial class App : Application
    {
        private static IHost? _host;

        public static T GetService<T>() where T : class
        {
            return _host?.Services.GetService<T>() ??
                   throw new ArgumentException(
                       $"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        public static IServiceProvider Services =>
            _host?.Services ?? throw new InvalidOperationException("Services not available");

        protected override async void OnStartup(StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Registrar la conexión a la base de datos
                    services.AddSingleton<Conexion>();

                    // Registrar servicios
                    services.AddTransient<IAuthService, AuthService>();
                    services.AddSingleton<IUserSessionService, UserSessionService>(); // Singleton para mantener estado 
                    // Registrar ViewModels
                    services.AddTransient<DatabaseLoginViewModel>();
                    services.AddTransient<ShellViewModel>(); // Corregido: era MainWindowViewModel
                    services.AddTransient<TopBarViewModel>();
                    
                    // Registrar ViewModels de páginas
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<CustomersViewModel>();
                    services.AddTransient<BooksViewModel>();
                    services.AddTransient<MembersViewModel>();
                    services.AddTransient<OrdersViewModel>();
                    services.AddTransient<LoanViewModel>();
                    services.AddTransient<CategoriesViewModel>();
                    services.AddTransient<AuthorsViewModel>();
                    services.AddTransient<HistorialViewModel>();


                    // Registrar ventanas
                    services.AddSingleton<MainWindow>();
                })
                .Build();
            ThemeService.Apply(false);
            await _host.StartAsync();
    
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}