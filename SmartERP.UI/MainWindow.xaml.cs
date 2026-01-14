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
        PageTitleText.Text = "Billing Management";
        ContentFrame.Navigate(new BillingPage(_unitOfWork, _authService));
    }

    private void ReportsButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveButton(ReportsButton);
        PageTitleText.Text = "Reports";
        ContentFrame.Navigate(new ReportsPage(_unitOfWork, _authService));
    }

    private void UserManagementButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveButton(UserManagementButton);
        PageTitleText.Text = "User Management";
        ContentFrame.Navigate(new UserManagementPage(_unitOfWork, _authService));
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

    private void NavigateToPage(string pageName)
    {
        switch (pageName)
        {
            case "Billing":
                BillingButton_Click(BillingButton, new RoutedEventArgs());
                break;
            case "Customers":
                CustomersButton_Click(CustomersButton, new RoutedEventArgs());
                break;
            case "Inventory":
                InventoryButton_Click(InventoryButton, new RoutedEventArgs());
                break;
            case "Reports":
                ReportsButton_Click(ReportsButton, new RoutedEventArgs());
                break;
        }
    }

    private void ShowDashboard()
    {
        PageTitleText.Text = "Dashboard";
        ContentFrame.Navigate(new DashboardPage(_unitOfWork, _authService, NavigateToPage));
    }
}