using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LicenseManagerMVVM.Models;
using LicenseManagerMVVM.ViewModels;
using LicenseManagerMVVM.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LicenseManagerMVVM
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ConfigureServices();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Define your connection string here or retrieve it from configuration
            var connectionString = "Host=localhost;Port=5432;Database=licensedb;User Id=licensedb_admin;Password=;Encoding=UTF8";

            // Register AppDbContext as a singleton (or scoped based on your needs)
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString), ServiceLifetime.Singleton);

            // Register other services, view models, etc.
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<MainWindow>();

            // Add more service registrations as needed

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();
        }
    }
}