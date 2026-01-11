using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartERP.Core.Services;
using SmartERP.Data;
using SmartERP.Data.Repositories;
using SmartERP.UI.Views;

namespace SmartERP.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public static IConfiguration Configuration { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        // Setup dependency injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Initialize database
        await InitializeDatabaseAsync();

        // Show login window
        var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        services.AddSingleton(Configuration);

        // Database Context - Use Singleton for WPF to avoid disposed context issues
        services.AddSingleton<SmartERPDbContext>(provider =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<SmartERPDbContext>();
            optionsBuilder.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            return new SmartERPDbContext(optionsBuilder.Options);
        });

        // Repositories
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IInventoryRepository, InventoryRepository>();
        services.AddSingleton<ICustomerRepository, CustomerRepository>();
        services.AddSingleton<IBillingRepository, BillingRepository>();

        // Unit of Work
        services.AddSingleton<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IAuthenticationService, AuthenticationService>();

        // Database Initializer
        services.AddTransient<DatabaseInitializer>();

        // Windows
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        
        await dbInitializer.InitializeAsync();
        await dbInitializer.SeedDataAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        base.OnExit(e);
    }
}

