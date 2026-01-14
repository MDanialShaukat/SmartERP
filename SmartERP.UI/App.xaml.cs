using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Threading;
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
    private static ILoggingService? _loggingService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup global exception handlers
        SetupExceptionHandlers();

        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        // Setup dependency injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Get logging service
        _loggingService = ServiceProvider.GetRequiredService<ILoggingService>();
        _loggingService.LogInformation("Application starting...", "App");

        try
        {
            // Initialize database
            await InitializeDatabaseAsync();
            _loggingService.LogInformation("Database initialized successfully", "App");

            // Show login window
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            _loggingService?.LogCritical("Application startup failed", ex, "App");
            MessageBox.Show($"Failed to start application: {ex.Message}\n\nPlease check the logs for details.",
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void SetupExceptionHandlers()
    {
        // Handle UI thread exceptions
        DispatcherUnhandledException += (s, e) =>
        {
            _loggingService?.LogError("Unhandled UI exception", e.Exception, "Dispatcher");
            
            MessageBox.Show($"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe error has been logged.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true; // Prevent application crash
        };

        // Handle non-UI thread exceptions
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            _loggingService?.LogCritical("Unhandled domain exception", exception, "AppDomain");
            
            if (exception != null)
            {
                MessageBox.Show($"A critical error occurred:\n\n{exception.Message}\n\nThe application will now close.",
                    "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        // Handle task exceptions
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            _loggingService?.LogError("Unobserved task exception", e.Exception, "TaskScheduler");
            e.SetObserved(); // Prevent application crash
        };
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        services.AddSingleton(Configuration);

        // Logging Service - Add first so other services can use it
        services.AddSingleton<ILoggingService, FileLoggingService>();

        // Database Context - Use Singleton for WPF to avoid disposed context issues
        services.AddSingleton<SmartERPDbContext>(provider =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<SmartERPDbContext>();
            optionsBuilder.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            return new SmartERPDbContext(optionsBuilder.Options);
        });

        // Backup Service
        services.AddSingleton<IBackupService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var logging = provider.GetRequiredService<ILoggingService>();
            return new DatabaseBackupService(config, (msg) => logging.LogInformation(msg, "BackupService"));
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
        _loggingService?.LogInformation("Application shutting down...", "App");
        
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        base.OnExit(e);
    }
}

