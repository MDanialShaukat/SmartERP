using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmartERP.Core.Services;
using SmartERP.Data;

namespace SmartERP.UI.Views
{
    public partial class DashboardPage : Page
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authService;
        private readonly Action<string> _navigateToPage;

        public DashboardPage(IUnitOfWork unitOfWork, IAuthenticationService authService, Action<string> navigateToPage)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _authService = authService;
            _navigateToPage = navigateToPage;

            Loaded += DashboardPage_Loaded;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDashboardDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDashboardDataAsync()
        {
            try
            {
                // Set welcome message
                if (_authService.CurrentUser != null)
                {
                    WelcomeText.Text = $"Welcome back, {_authService.CurrentUser.FullName}!";
                }

                // Set current date and time
                DateTimeText.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - hh:mm tt");
                CurrentMonthText.Text = DateTime.Now.ToString("MMMM yyyy");

                // Load all data
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var billings = await _unitOfWork.Billings.GetAllAsync();
                var inventory = await _unitOfWork.Inventories.GetAllAsync();
                var users = await _unitOfWork.Users.GetAllAsync();

                // Customer Metrics
                var totalCustomers = customers.Count();
                var activeCustomers = customers.Count(c => c.IsActive);
                TotalCustomersText.Text = totalCustomers.ToString();
                ActiveCustomersText.Text = $"{activeCustomers} active";

                // Monthly Revenue
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var monthlyBills = billings.Where(b => b.BillingMonth == currentMonth && b.BillingYear == currentYear).ToList();
                var monthlyRevenue = monthlyBills.Sum(b => b.AmountPaid);
                MonthlyRevenueText.Text = $"Rs. {monthlyRevenue:N0}";
                MonthlyBillsText.Text = $"{monthlyBills.Count} bills";

                // Outstanding Amount
                var totalOutstanding = customers.Sum(c => c.OutstandingBalance);
                var overdueBills = billings.Where(b =>
                    b.DueDate < DateTime.Now.Date &&
                    (b.PaymentStatus == "Pending" || b.PaymentStatus == "Partial")).ToList();
                OutstandingAmountText.Text = $"Rs. {totalOutstanding:N0}";
                OverdueBillsText.Text = $"{overdueBills.Count} overdue";

                // Inventory Metrics
                var lowStockItems = inventory.Count(i => i.QuantityRemaining <= 10);
                var totalInventoryValue = inventory.Sum(i => i.QuantityRemaining * i.PurchasePrice);
                LowStockItemsText.Text = lowStockItems.ToString();
                InventoryValueText.Text = $"Rs. {totalInventoryValue:N0} total value";

                // Recent Overdue Bills
                if (overdueBills.Any())
                {
                    var recentOverdue = overdueBills
                        .OrderByDescending(b => (DateTime.Now.Date - b.DueDate.Date).Days)
                        .Take(5)
                        .Select(b => new
                        {
                            BillNumber = b.BillNumber,
                            CustomerName = b.Customer?.CustomerName ?? "N/A",
                            Balance = b.BalanceAmount,
                            DueDate = b.DueDate,
                            DaysOverdue = (DateTime.Now.Date - b.DueDate.Date).Days
                        })
                        .ToList();

                    RecentOverdueGrid.ItemsSource = recentOverdue;
                    RecentOverdueGrid.Visibility = Visibility.Visible;
                    NoOverdueText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    RecentOverdueGrid.Visibility = Visibility.Collapsed;
                    NoOverdueText.Visibility = Visibility.Visible;
                }

                // System Status
                UserCountText.Text = $"Active Users: {users.Count(u => u.IsActive)}";

                // Payment Summary
                var paidBills = monthlyBills.Count(b => b.PaymentStatus == "Paid");
                var pendingBills = monthlyBills.Count(b => b.PaymentStatus != "Paid");
                PaidBillsCountText.Text = paidBills.ToString();
                PendingBillsCountText.Text = pendingBills.ToString();

                // Inventory Summary
                TotalItemsText.Text = inventory.Count().ToString();
                LowStockCountText.Text = lowStockItems.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Quick Action Handlers
        private void AddBill_Click(object sender, MouseButtonEventArgs e)
        {
            _navigateToPage("Billing");
            // Optionally open add dialog immediately
            var billingPage = new BillingPage(_unitOfWork, _authService);
            // Trigger add button programmatically if needed
        }

        private void AddCustomer_Click(object sender, MouseButtonEventArgs e)
        {
            _navigateToPage("Customers");
        }

        private void AddInventory_Click(object sender, MouseButtonEventArgs e)
        {
            _navigateToPage("Inventory");
        }

        private void ViewReports_Click(object sender, MouseButtonEventArgs e)
        {
            _navigateToPage("Reports");
        }

        private void ViewAllOverdue_Click(object sender, RoutedEventArgs e)
        {
            _navigateToPage("Billing");
        }
    }
}
