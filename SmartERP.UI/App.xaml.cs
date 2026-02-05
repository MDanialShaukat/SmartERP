using System.Configuration;
using System.Data;
using System.Diagnostics;
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
    private static IDatabaseSecurityService? _databaseSecurityService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup global exception handlers
        SetupExceptionHandlers();

        // Ensure required directories exist
        EnsureRequiredDirectories();

        // Build configuration with fallback
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        Configuration = builder.Build();

        // Validate configuration
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            MessageBox.Show(
                "Configuration Error: appsettings.json is missing or misconfigured.\n\n" +
                "Please ensure appsettings.json exists in the application directory: " + 
                Directory.GetCurrentDirectory() + "\n\n" +
                "Application will now exit.",
                "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        // Setup dependency injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Get logging service
        _loggingService = ServiceProvider.GetRequiredService<ILoggingService>();
        _loggingService.LogInformation("Application starting...", "App");
        _loggingService.LogInformation($"Application Directory: {Directory.GetCurrentDirectory()}", "App");
        _loggingService.LogInformation($"Connection String: {connectionString}", "App");

        try
        {
            // Initialize database
            await InitializeDatabaseAsync();
            _loggingService.LogInformation("Database initialized successfully", "App");

            // Secure database
            await SecureDatabaseAsync();

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

    private void EnsureRequiredDirectories()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var directories = new[] { "Logs", "Data", "Backups" };

            foreach (var dir in directories)
            {
                var fullPath = Path.Combine(baseDir, dir);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }
        }
        catch (Exception ex)
        {
            // If directory creation fails, log it but don't crash
            Debug.WriteLine($"Failed to create required directories: {ex.Message}");
        }
    }

    private async Task SecureDatabaseAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var secureDatabaseOnStartup = bool.Parse(Configuration["Security:SecureDatabaseOnStartup"] ?? "true");
                var verifyIntegrity = bool.Parse(Configuration["Security:VerifyDatabaseIntegrity"] ?? "true");

                if (!secureDatabaseOnStartup && !verifyIntegrity)
                    return;

                _databaseSecurityService = ServiceProvider.GetService<IDatabaseSecurityService>();
                if (_databaseSecurityService == null)
                {
                    _loggingService?.LogWarning("Database security service not available", "App");
                    return;
                }

                var databasePath = Configuration.GetConnectionString("DefaultConnection")
                    ?.Replace("Data Source=", "") ?? "SmartERP.db";

                // Verify database integrity
                if (verifyIntegrity)
                {
                    var isValid = _databaseSecurityService.VerifyDatabaseIntegrity(databasePath);
                    if (!isValid)
                    {
                        _loggingService?.LogWarning("Database integrity check failed - database may be corrupted", "App");
                    }
                }

                // Secure database file
                if (secureDatabaseOnStartup)
                {
                    _databaseSecurityService.SecureDatabaseFile(databasePath);
                }

                _loggingService?.LogInformation("Database security checks completed", "App");
            }
            catch (Exception ex)
            {
                _loggingService?.LogError("Database security initialization failed", ex, "App");
            }
        });
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
        // Use "Logs" folder in the application directory
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Directory.CreateDirectory(logPath); // Ensure directory exists
        services.AddSingleton<ILoggingService>(new FileLoggingService(logPath));

        // Encryption Service
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Database Security Service
        services.AddSingleton<IDatabaseSecurityService, DatabaseSecurityService>();

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
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            
            _loggingService?.LogInformation("Starting database initialization...", "DatabaseInit");
            await dbInitializer.InitializeAsync();
            _loggingService?.LogInformation("Database structure created/verified", "DatabaseInit");
            
            await dbInitializer.SeedDataAsync();
            _loggingService?.LogInformation("Database seeding completed", "DatabaseInit");
        }
        catch (FileNotFoundException fnex)
        {
            _loggingService?.LogError("Configuration file not found", fnex, "DatabaseInit");
            throw new InvalidOperationException(
                "Failed to initialize database: Configuration file (appsettings.json) is missing.\n\n" +
                "Please ensure appsettings.json exists in the application directory.\n" +
                "Current Directory: " + Directory.GetCurrentDirectory(), fnex);
        }
        catch (InvalidOperationException ioex) when (ioex.Message.Contains("appsettings"))
        {
            _loggingService?.LogError("Configuration error", ioex, "DatabaseInit");
            throw;
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("Database initialization failed", ex, "DatabaseInit");
            throw new InvalidOperationException(
                "Failed to initialize database.\n\n" +
                "Error: " + ex.Message + "\n\n" +
                "Please check that:\n" +
                "1. appsettings.json exists and is properly configured\n" +
                "2. The database path is writable\n" +
                "3. All required directories exist (Logs, Data, Backups)", ex);
        }
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

