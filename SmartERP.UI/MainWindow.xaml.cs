using System;
using System.Windows;
using System.Windows.Controls;
using SmartERP.Core.Services;
using SmartERP.Data;
using SmartERP.UI.Views;

namespace SmartERP.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IAuthenticationService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private Button? _activeButton;

    public MainWindow(IAuthenticationService authService, IUnitOfWork unitOfWork)
    {
        InitializeComponent();
        _authService = authService;
        _unitOfWork = unitOfWork;
        
        // Display user info in title
        if (_authService.CurrentUser != null)
        {
            Title = $"SmartERP - Welcome {_authService.CurrentUser.FullName} ({_authService.CurrentUser.Role})";
            UserInfoText.Text = $"{_authService.CurrentUser.FullName} ({_authService.CurrentUser.Role})";
            
            // Show admin-only features
            if (_authService.IsAdmin)
            {
                ReportsButton.Visibility = Visibility.Visible;
                UserManagementButton.Visibility = Visibility.Visible;
            }
        }

        // Set current date
        CurrentDateText.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");

        // Set Dashboard as active by default
        _activeButton = DashboardButton;
        ShowDashboard();
    }

    private void SetActiveButton(Button button)
    {
        // Reset previous active button
        if (_activeButton != null)
        {
            _activeButton.Style = (Style)FindResource("MenuButtonStyle");
        }

        // Set new active button
        _activeButton = button;
        _activeButton.Style = (Style)FindResource("ActiveMenuButtonStyle");
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveButton(DashboardButton);
        ShowDashboard();
    }

    private void InventoryButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveButton(InventoryButton);
        PageTitleText.Text = "Inventory Management";
        ContentFrame.Navigate(new InventoryPage(_unitOfWork, _authService));
    }

    private void CustomersButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveButton(CustomersButton);
        PageTitleText.Text = "Customer Management";
        ContentFrame.Navigate(new CustomerPage(_unitOfWork, _authService));
    }

    private void BillingButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveButton(BillingButton);
        PageTitleText.Text = "Billing";
        // TODO: Navigate to Billing page
        ContentFrame.Content = new TextBlock 
        { 
            Text = "Billing - Coming Soon", 
            FontSize = 24, 
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void ReportsButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveButton(ReportsButton);
        PageTitleText.Text = "Reports";
        // TODO: Navigate to Reports page
        ContentFrame.Content = new TextBlock 
        { 
            Text = "Reports - Coming Soon", 
            FontSize = 24, 
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void UserManagementButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveButton(UserManagementButton);
        PageTitleText.Text = "User Management";
        // TODO: Navigate to User Management page
        ContentFrame.Content = new TextBlock 
        { 
            Text = "User Management - Coming Soon", 
            FontSize = 24, 
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to logout?",
            "Confirm Logout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _authService.Logout();
            
            // Show login window
            var loginWindow = new LoginWindow(_authService);
            loginWindow.Show();
            
            // Close main window
            this.Close();
        }
    }

    private void ShowDashboard()
    {
        PageTitleText.Text = "Dashboard";
        
        // Create simple dashboard
        var dashboardGrid = new Grid { Margin = new Thickness(20) };
        dashboardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        
        var welcomeText = new TextBlock
        {
            Text = $"Welcome back, {_authService.CurrentUser?.FullName}!",
            FontSize = 28,
            FontWeight = FontWeights.SemiBold,
            Foreground = System.Windows.Media.Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        dashboardGrid.Children.Add(welcomeText);
        ContentFrame.Content = dashboardGrid;
    }
}