using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;
using SmartERP.Core.Services;
using SmartERP.Data;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class ReportViewerDialog : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authService;
        private readonly ReportType _reportType;
        private List<object> _reportData;

        public ReportViewerDialog(IUnitOfWork unitOfWork, IAuthenticationService authService, ReportType reportType)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _authService = authService;
            _reportType = reportType;
            _reportData = new List<object>();

            InitializeReport();
        }

        private void InitializeReport()
        {
            // Set report title and configure filters
            switch (_reportType)
            {
                case ReportType.MonthlyBilling:
                    ReportTitle.Text = "Monthly Billing Summary Report";
                    MonthYearPanel.Visibility = Visibility.Visible;
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.OverdueBills:
                    ReportTitle.Text = "Overdue Bills Report";
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.PaymentCollection:
                    ReportTitle.Text = "Payment Collection Report";
                    DateRangePanel.Visibility = Visibility.Visible;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.ActiveCustomers:
                    ReportTitle.Text = "Active Customers Report";
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.CustomerOutstanding:
                    ReportTitle.Text = "Customer Outstanding Report";
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.NewCustomers:
                    ReportTitle.Text = "New Customers Report";
                    DateRangePanel.Visibility = Visibility.Visible;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.StockStatus:
                    ReportTitle.Text = "Stock Status Report";
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.LowStock:
                    ReportTitle.Text = "Low Stock Alert Report";
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.InventoryValue:
                    ReportTitle.Text = "Inventory Valuation Report";
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.AssignmentSummary:
                    ReportTitle.Text = "Inventory Assignment Summary Report";
                    DateRangePanel.Visibility = Visibility.Visible;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.AssignmentByUser:
                    ReportTitle.Text = "Inventory Assignments by User Report";
                    DateRangePanel.Visibility = Visibility.Visible;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.AssignmentByItem:
                    ReportTitle.Text = "Inventory Assignments by Item Report";
                    DateRangePanel.Visibility = Visibility.Visible;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.AssignmentDetails:
                    ReportTitle.Text = "Inventory Assignment Details Report";
                    DateRangePanel.Visibility = Visibility.Visible;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.AssignmentTrend:
                    ReportTitle.Text = "Inventory Assignment Trends Report";
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.UserActivity:
                    ReportTitle.Text = "User Activity Report";
                    DateRangePanel.Visibility = Visibility.Visible;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;

                case ReportType.DashboardSummary:
                    ReportTitle.Text = "Dashboard Summary Report";
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    MonthYearPanel.Visibility = Visibility.Collapsed;
                    break;
            }

            // Initialize Month/Year dropdowns
            InitializeMonthYear();

            // Initialize date range
            FromDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            ToDatePicker.SelectedDate = DateTime.Now;
        }

        private void InitializeMonthYear()
        {
            // Set current month
            MonthComboBox.SelectedIndex = DateTime.Now.Month - 1;

            // Populate years
            var currentYear = DateTime.Now.Year;
            for (int i = 0; i < 5; i++)
            {
                var year = currentYear - i;
                YearComboBox.Items.Add(new ComboBoxItem { Content = year.ToString(), Tag = year });
            }
            YearComboBox.SelectedIndex = 0;
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Generating report...";
                ReportDataGrid.Columns.Clear();
                _reportData.Clear();

                switch (_reportType)
                {
                    case ReportType.MonthlyBilling:
                        await GenerateMonthlyBillingReport();
                        break;
                    case ReportType.OverdueBills:
                        await GenerateOverdueBillsReport();
                        break;
                    case ReportType.PaymentCollection:
                        await GeneratePaymentCollectionReport();
                        break;
                    case ReportType.ActiveCustomers:
                        await GenerateActiveCustomersReport();
                        break;
                    case ReportType.CustomerOutstanding:
                        await GenerateCustomerOutstandingReport();
                        break;
                    case ReportType.NewCustomers:
                        await GenerateNewCustomersReport();
                        break;
                    case ReportType.StockStatus:
                        await GenerateStockStatusReport();
                        break;
                    case ReportType.LowStock:
                        await GenerateLowStockReport();
                        break;
                    case ReportType.InventoryValue:
                        await GenerateInventoryValueReport();
                        break;
                    case ReportType.AssignmentSummary:
                        await GenerateAssignmentSummaryReport();
                        break;
                    case ReportType.AssignmentByUser:
                        await GenerateAssignmentByUserReport();
                        break;
                    case ReportType.AssignmentByItem:
                        await GenerateAssignmentByItemReport();
                        break;
                    case ReportType.AssignmentDetails:
                        await GenerateAssignmentDetailsReport();
                        break;
                    case ReportType.AssignmentTrend:
                        await GenerateAssignmentTrendReport();
                        break;
                    case ReportType.UserActivity:
                        await GenerateUserActivityReport();
                        break;
                    case ReportType.DashboardSummary:
                        await GenerateDashboardSummaryReport();
                        break;
                }

                RecordCountText.Text = $"Total Records: {_reportData.Count}";
                StatusText.Text = "Report generated successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error generating report";
            }
        }

        // Report Generation Methods

        private async System.Threading.Tasks.Task GenerateMonthlyBillingReport()
        {
            var month = int.Parse(((ComboBoxItem)MonthComboBox.SelectedItem).Tag.ToString()!);
            var year = int.Parse(((ComboBoxItem)YearComboBox.SelectedItem).Tag.ToString()!);

            var billings = await _unitOfWork.Billings.GetBillsByMonthYearAsync(month, year);
            
            var reportData = billings.Select(b => new
            {
                BillNumber = b.BillNumber,
                CustomerName = b.Customer?.CustomerName ?? "N/A",
                BillDate = b.BillDate.ToString("dd/MM/yyyy"),
                BillAmount = b.BillAmount,
                PreviousDue = b.PreviousDue,
                TotalAmount = b.TotalAmount,
                AmountPaid = b.AmountPaid,
                BalanceAmount = b.BalanceAmount,
                PaymentStatus = b.PaymentStatus,
                PaymentMethod = b.PaymentMethod,
                DueDate = b.DueDate.ToString("dd/MM/yyyy")
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            // Add columns
            AddDataGridColumn("Bill Number", "BillNumber");
            AddDataGridColumn("Customer", "CustomerName");
            AddDataGridColumn("Bill Date", "BillDate");
            AddDataGridColumn("Bill Amount", "BillAmount", "C2");
            AddDataGridColumn("Previous Due", "PreviousDue", "C2");
            AddDataGridColumn("Total Amount", "TotalAmount", "C2");
            AddDataGridColumn("Paid", "AmountPaid", "C2");
            AddDataGridColumn("Balance", "BalanceAmount", "C2");
            AddDataGridColumn("Status", "PaymentStatus");
            AddDataGridColumn("Payment Method", "PaymentMethod");
            AddDataGridColumn("Due Date", "DueDate");

            // Show summary
            ShowMonthlySummary(billings);
        }

        private void ShowMonthlySummary(IEnumerable<Billing> billings)
        {
            SummaryPanel.Visibility = Visibility.Visible;
            SummaryContent.Children.Clear();

            var totalBills = billings.Count();
            var totalAmount = billings.Sum(b => b.TotalAmount);
            var totalPaid = billings.Sum(b => b.AmountPaid);
            var totalOutstanding = billings.Sum(b => b.BalanceAmount);
            var paidBills = billings.Count(b => b.PaymentStatus == "Paid");
            var pendingBills = billings.Count(b => b.PaymentStatus != "Paid");

            AddSummaryItem("Total Bills", totalBills.ToString());
            AddSummaryItem("Total Amount", $"Rs. {totalAmount:N2}");
            AddSummaryItem("Total Paid", $"Rs. {totalPaid:N2}");
            AddSummaryItem("Total Outstanding", $"Rs. {totalOutstanding:N2}");
            AddSummaryItem("Paid Bills", paidBills.ToString());
            AddSummaryItem("Pending Bills", pendingBills.ToString());
        }

        private void AddSummaryItem(string label, string value)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var labelText = new TextBlock
            {
                Text = label + ":",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.Gray
            };
            Grid.SetColumn(labelText, 0);

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Black
            };
            Grid.SetColumn(valueText, 1);

            grid.Children.Add(labelText);
            grid.Children.Add(valueText);
            SummaryContent.Children.Add(grid);
        }

        private async System.Threading.Tasks.Task GenerateOverdueBillsReport()
        {
            var bills = await _unitOfWork.Billings.GetOverdueBillsAsync();
            
            var reportData = bills.Select(b => new
            {
                BillNumber = b.BillNumber,
                CustomerName = b.Customer?.CustomerName ?? "N/A",
                CustomerPhone = b.Customer?.PhoneNumber ?? "N/A",
                BillDate = b.BillDate.ToString("dd/MM/yyyy"),
                DueDate = b.DueDate.ToString("dd/MM/yyyy"),
                DaysOverdue = (DateTime.Now.Date - b.DueDate.Date).Days,
                TotalAmount = b.TotalAmount,
                AmountPaid = b.AmountPaid,
                Balance = b.BalanceAmount,
                PaymentStatus = b.PaymentStatus
            }).OrderByDescending(b => b.DaysOverdue).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Bill Number", "BillNumber");
            AddDataGridColumn("Customer", "CustomerName");
            AddDataGridColumn("Phone", "CustomerPhone");
            AddDataGridColumn("Bill Date", "BillDate");
            AddDataGridColumn("Due Date", "DueDate");
            AddDataGridColumn("Days Overdue", "DaysOverdue");
            AddDataGridColumn("Total Amount", "TotalAmount", "C2");
            AddDataGridColumn("Paid", "AmountPaid", "C2");
            AddDataGridColumn("Balance", "Balance", "C2");
            AddDataGridColumn("Status", "PaymentStatus");
        }

        private async System.Threading.Tasks.Task GeneratePaymentCollectionReport()
        {
            var fromDate = FromDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var toDate = ToDatePicker.SelectedDate ?? DateTime.Now;

            var allBillings = await _unitOfWork.Billings.GetAllAsync();
            var payments = allBillings.Where(b => 
                b.PaymentDate.HasValue && 
                b.PaymentDate.Value.Date >= fromDate.Date && 
                b.PaymentDate.Value.Date <= toDate.Date)
                .ToList();

            var reportData = payments.Select(b => new
            {
                BillNumber = b.BillNumber,
                CustomerName = b.Customer?.CustomerName ?? "N/A",
                PaymentDate = b.PaymentDate?.ToString("dd/MM/yyyy") ?? "N/A",
                PaymentMethod = b.PaymentMethod,
                AmountPaid = b.AmountPaid,
                TransactionRef = b.TransactionReference
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Bill Number", "BillNumber");
            AddDataGridColumn("Customer", "CustomerName");
            AddDataGridColumn("Payment Date", "PaymentDate");
            AddDataGridColumn("Payment Method", "PaymentMethod");
            AddDataGridColumn("Amount Paid", "AmountPaid", "C2");
            AddDataGridColumn("Transaction Ref", "TransactionRef");

            // Show payment summary
            SummaryPanel.Visibility = Visibility.Visible;
            SummaryContent.Children.Clear();
            AddSummaryItem("Total Payments", payments.Count.ToString());
            AddSummaryItem("Total Amount Collected", $"Rs. {payments.Sum(p => p.AmountPaid):N2}");
            
            var byMethod = payments.GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Amount = g.Sum(p => p.AmountPaid) });
            
            foreach (var method in byMethod)
            {
                AddSummaryItem($"{method.Method} Payments", $"Rs. {method.Amount:N2}");
            }
        }

        private async System.Threading.Tasks.Task GenerateActiveCustomersReport()
        {
            var customers = await _unitOfWork.Customers.GetActiveCustomersAsync();
            
            var reportData = customers.Select(c => new
            {
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                Area = c.Area?.AreaName ?? "N/A",
                PackageType = c.PackageType,
                PackageAmount = c.PackageAmount,
                OutstandingBalance = c.OutstandingBalance,
                ConnectionDate = c.ConnectionDate.ToString("dd/MM/yyyy")
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Code", "CustomerCode");
            AddDataGridColumn("Name", "CustomerName");
            AddDataGridColumn("Phone", "PhoneNumber");
            AddDataGridColumn("Email", "Email");
            AddDataGridColumn("Area", "Area");
            AddDataGridColumn("Package", "PackageType");
            AddDataGridColumn("Package Amount", "PackageAmount", "C2");
            AddDataGridColumn("Outstanding", "OutstandingBalance", "C2");
            AddDataGridColumn("Connection Date", "ConnectionDate");
        }

        private async System.Threading.Tasks.Task GenerateCustomerOutstandingReport()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            var customersWithDue = customers.Where(c => c.OutstandingBalance > 0)
                .OrderByDescending(c => c.OutstandingBalance)
                .ToList();

            var reportData = customersWithDue.Select(c => new
            {
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                PhoneNumber = c.PhoneNumber,
                Area = c.Area?.AreaName ?? "N/A",
                PackageAmount = c.PackageAmount,
                OutstandingBalance = c.OutstandingBalance,
                IsActive = c.IsActive ? "Yes" : "No"
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Code", "CustomerCode");
            AddDataGridColumn("Name", "CustomerName");
            AddDataGridColumn("Phone", "PhoneNumber");
            AddDataGridColumn("Area", "Area");
            AddDataGridColumn("Package Amount", "PackageAmount", "C2");
            AddDataGridColumn("Outstanding", "OutstandingBalance", "C2");
            AddDataGridColumn("Active", "IsActive");

            // Summary
            SummaryPanel.Visibility = Visibility.Visible;
            SummaryContent.Children.Clear();
            AddSummaryItem("Customers with Outstanding", customersWithDue.Count.ToString());
            AddSummaryItem("Total Outstanding Amount", $"Rs. {customersWithDue.Sum(c => c.OutstandingBalance):N2}");
        }

        private async System.Threading.Tasks.Task GenerateNewCustomersReport()
        {
            var fromDate = FromDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var toDate = ToDatePicker.SelectedDate ?? DateTime.Now;

            var allCustomers = await _unitOfWork.Customers.GetAllAsync();
            var newCustomers = allCustomers.Where(c =>
                c.CreatedDate.Date >= fromDate.Date &&
                c.CreatedDate.Date <= toDate.Date)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            var reportData = newCustomers.Select(c => new
            {
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                Area = c.Area?.AreaName ?? "N/A",
                PackageType = c.PackageType,
                PackageAmount = c.PackageAmount,
                ConnectionDate = c.ConnectionDate.ToString("dd/MM/yyyy"),
                CreatedDate = c.CreatedDate.ToString("dd/MM/yyyy")
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Code", "CustomerCode");
            AddDataGridColumn("Name", "CustomerName");
            AddDataGridColumn("Phone", "PhoneNumber");
            AddDataGridColumn("Email", "Email");
            AddDataGridColumn("Area", "Area");
            AddDataGridColumn("Package", "PackageType");
            AddDataGridColumn("Package Amount", "PackageAmount", "C2");
            AddDataGridColumn("Connection Date", "ConnectionDate");
            AddDataGridColumn("Created Date", "CreatedDate");
        }

        private async System.Threading.Tasks.Task GenerateStockStatusReport()
        {
            var items = await _unitOfWork.Inventories.GetAllAsync();
            
            var reportData = items.Select(i => new
            {
                ItemName = i.ItemName,
                Category = i.Category,
                Unit = i.Unit,
                PurchasePrice = i.PurchasePrice,
                QuantityPurchased = i.QuantityPurchased,
                QuantityUsed = i.QuantityUsed,
                QuantityRemaining = i.QuantityRemaining,
                TotalValue = i.QuantityRemaining * i.PurchasePrice,
                Status = i.QuantityRemaining <= 10 ? "Low Stock" : "Normal",
                Supplier = i.Supplier
            }).OrderBy(i => i.QuantityRemaining).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Item Name", "ItemName");
            AddDataGridColumn("Category", "Category");
            AddDataGridColumn("Unit", "Unit");
            AddDataGridColumn("Purchase Price", "PurchasePrice", "C2");
            AddDataGridColumn("Purchased", "QuantityPurchased");
            AddDataGridColumn("Used", "QuantityUsed");
            AddDataGridColumn("Remaining", "QuantityRemaining");
            AddDataGridColumn("Total Value", "TotalValue", "C2");
            AddDataGridColumn("Status", "Status");
            AddDataGridColumn("Supplier", "Supplier");
        }

        private async System.Threading.Tasks.Task GenerateLowStockReport()
        {
            var items = await _unitOfWork.Inventories.GetLowStockItemsAsync(10);
            
            var reportData = items.Select(i => new
            {
                ItemName = i.ItemName,
                Category = i.Category,
                Unit = i.Unit,
                QuantityRemaining = i.QuantityRemaining,
                PurchasePrice = i.PurchasePrice,
                ReorderValue = i.QuantityRemaining * i.PurchasePrice,
                Supplier = i.Supplier,
                LastPurchaseDate = i.PurchaseDate.ToString("dd/MM/yyyy")
            }).OrderBy(i => i.QuantityRemaining).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Item Name", "ItemName");
            AddDataGridColumn("Category", "Category");
            AddDataGridColumn("Unit", "Unit");
            AddDataGridColumn("Qty Remaining", "QuantityRemaining");
            AddDataGridColumn("Purchase Price", "PurchasePrice", "C2");
            AddDataGridColumn("Reorder Value", "ReorderValue", "C2");
            AddDataGridColumn("Supplier", "Supplier");
            AddDataGridColumn("Last Purchase", "LastPurchaseDate");
        }

        private async System.Threading.Tasks.Task GenerateInventoryValueReport()
        {
            var items = await _unitOfWork.Inventories.GetAllAsync();
            
            var reportData = items.Select(i => new
            {
                ItemName = i.ItemName,
                Category = i.Category,
                QuantityRemaining = i.QuantityRemaining,
                PurchasePrice = i.PurchasePrice,
                TotalValue = i.QuantityRemaining * i.PurchasePrice
            }).OrderByDescending(i => i.TotalValue).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Item Name", "ItemName");
            AddDataGridColumn("Category", "Category");
            AddDataGridColumn("Qty Remaining", "QuantityRemaining");
            AddDataGridColumn("Unit Price", "PurchasePrice", "C2");
            AddDataGridColumn("Total Value", "TotalValue", "C2");

            // Summary
            SummaryPanel.Visibility = Visibility.Visible;
            SummaryContent.Children.Clear();
            AddSummaryItem("Total Items", items.Count().ToString());
            AddSummaryItem("Total Inventory Value", $"Rs. {reportData.Sum(i => i.TotalValue):N2}");
            
            var byCategory = items.GroupBy(i => i.Category)
                .Select(g => new { Category = g.Key, Value = g.Sum(i => i.QuantityRemaining * i.PurchasePrice) });
            
            foreach (var cat in byCategory)
            {
                AddSummaryItem($"{cat.Category} Value", $"Rs. {cat.Value:N2}");
            }
        }

        private async System.Threading.Tasks.Task GenerateUserActivityReport()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            
            var reportData = users.Select(u => new
            {
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role,
                Email = u.Email,
                IsActive = u.IsActive ? "Yes" : "No",
                LastLogin = u.LastLoginDate?.ToString("dd/MM/yyyy HH:mm") ?? "Never",
                CreatedDate = u.CreatedDate.ToString("dd/MM/yyyy")
            }).OrderByDescending(u => u.LastLogin).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Username", "Username");
            AddDataGridColumn("Full Name", "FullName");
            AddDataGridColumn("Role", "Role");
            AddDataGridColumn("Email", "Email");
            AddDataGridColumn("Active", "IsActive");
            AddDataGridColumn("Last Login", "LastLogin");
            AddDataGridColumn("Created Date", "CreatedDate");
        }

        private async System.Threading.Tasks.Task GenerateDashboardSummaryReport()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            var billings = await _unitOfWork.Billings.GetAllAsync();
            var inventory = await _unitOfWork.Inventories.GetAllAsync();
            var users = await _unitOfWork.Users.GetAllAsync();

            SummaryPanel.Visibility = Visibility.Visible;
            SummaryContent.Children.Clear();

            // Inventory Statistics
            var totalInventoryValue = inventory.Sum(i => i.QuantityRemaining * i.PurchasePrice);
            var lowStockItems = inventory.Count(i => i.QuantityRemaining <= 10);

            AddSummaryItem("Total Inventory Items", inventory.Count().ToString());
            AddSummaryItem("Total Inventory Value", $"Rs. {totalInventoryValue:N2}");
            AddSummaryItem("Low Stock Items", lowStockItems.ToString());

            // User Statistics
            AddSummaryItem("Total Users", users.Count().ToString());
            AddSummaryItem("Active Users", users.Count(u => u.IsActive).ToString());
            AddSummaryItem("Administrators", users.Count(u => u.Role == "Admin").ToString());

            _reportData.Clear();
            ReportDataGrid.ItemsSource = null;
        }

        // Inventory Assignment Report Methods

        private async System.Threading.Tasks.Task GenerateAssignmentSummaryReport()
        {
            var fromDate = FromDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var toDate = ToDatePicker.SelectedDate ?? DateTime.Now;

            var reportService = new InventoryAssignmentReportService(_unitOfWork);
            var summary = await reportService.GetAssignmentSummaryAsync(fromDate, toDate);

            SummaryPanel.Visibility = Visibility.Visible;
            SummaryContent.Children.Clear();

            AddSummaryItem("Report Period", $"{fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}");
            AddSummaryItem("Total Assignments", summary.TotalAssignments.ToString());
            AddSummaryItem("Total Quantity Assigned", summary.TotalQuantityAssigned.ToString());
            AddSummaryItem("Unique Items Assigned", summary.UniqueItemsAssigned.ToString());
            AddSummaryItem("Unique Users Assigned", summary.UniqueUsersAssigned.ToString());
            AddSummaryItem("Average Quantity per Assignment", $"{summary.AverageQuantityPerAssignment:F2}");
            AddSummaryItem("Average Assignments per Day", $"{summary.AverageAssignmentsPerDay:F2}");

            _reportData.Clear();
            ReportDataGrid.ItemsSource = null;
        }

        private async System.Threading.Tasks.Task GenerateAssignmentByUserReport()
        {
            var fromDate = FromDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var toDate = ToDatePicker.SelectedDate ?? DateTime.Now;

            var reportService = new InventoryAssignmentReportService(_unitOfWork);
            var userReports = await reportService.GetAssignmentsByUserAsync(fromDate, toDate);

            var reportData = userReports.Select(r => new
            {
                User = r.UserName,
                TotalAssignments = r.TotalAssignments,
                TotalQuantity = r.TotalQuantityReceived,
                UniqueItems = r.UniqueItemsReceived,
                FirstAssignment = r.FirstAssignmentDate.ToString("dd/MM/yyyy"),
                LastAssignment = r.LastAssignmentDate.ToString("dd/MM/yyyy"),
                AverageQuantity = r.AverageQuantityPerAssignment.ToString("F2")
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("User Name", "User");
            AddDataGridColumn("Total Assignments", "TotalAssignments");
            AddDataGridColumn("Total Quantity", "TotalQuantity");
            AddDataGridColumn("Unique Items", "UniqueItems");
            AddDataGridColumn("First Assignment", "FirstAssignment");
            AddDataGridColumn("Last Assignment", "LastAssignment");
            AddDataGridColumn("Average Quantity", "AverageQuantity");
        }

        private async System.Threading.Tasks.Task GenerateAssignmentByItemReport()
        {
            var fromDate = FromDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var toDate = ToDatePicker.SelectedDate ?? DateTime.Now;

            var reportService = new InventoryAssignmentReportService(_unitOfWork);
            var itemReports = await reportService.GetAssignmentsByItemAsync(fromDate, toDate);

            var reportData = itemReports.Select(r => new
            {
                ItemName = r.ItemName,
                Category = r.Category,
                TotalAssignments = r.TotalAssignments,
                TotalQuantity = r.TotalQuantityAssigned,
                UniqueUsers = r.UniqueUsersReceived,
                CurrentStock = r.CurrentStock,
                AverageQuantity = r.AverageQuantityPerAssignment.ToString("F2")
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Item Name", "ItemName");
            AddDataGridColumn("Category", "Category");
            AddDataGridColumn("Total Assignments", "TotalAssignments");
            AddDataGridColumn("Total Quantity Assigned", "TotalQuantity");
            AddDataGridColumn("Unique Users", "UniqueUsers");
            AddDataGridColumn("Current Stock", "CurrentStock");
            AddDataGridColumn("Average Qty/Assignment", "AverageQuantity");
        }

        private async System.Threading.Tasks.Task GenerateAssignmentDetailsReport()
        {
            var fromDate = FromDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-3);
            var toDate = ToDatePicker.SelectedDate ?? DateTime.Now;

            var reportService = new InventoryAssignmentReportService(_unitOfWork);
            var details = await reportService.GetDetailedAssignmentsAsync(fromDate, toDate);

            var reportData = details.Select(d => new
            {
                Item = d.ItemName,
                Category = d.Category,
                Quantity = d.QuantityAssigned,
                AssignedTo = d.AssignedToUser,
                AssignmentDate = d.AssignmentDate.ToString("dd/MM/yyyy HH:mm"),
                AssignedBy = d.AssignedByUser,
                Remarks = d.Remarks
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Item Name", "Item");
            AddDataGridColumn("Category", "Category");
            AddDataGridColumn("Quantity", "Quantity");
            AddDataGridColumn("Assigned To", "AssignedTo");
            AddDataGridColumn("Assignment Date", "AssignmentDate");
            AddDataGridColumn("Assigned By", "AssignedBy");
            AddDataGridColumn("Remarks", "Remarks");
        }

        private async System.Threading.Tasks.Task GenerateAssignmentTrendReport()
        {
            var reportService = new InventoryAssignmentReportService(_unitOfWork);
            var trend = await reportService.GetAssignmentTrendAsync(12);

            var reportData = trend.MonthlyData.Select(m => new
            {
                Month = m.Month,
                Assignments = m.AssignmentCount,
                TotalQuantity = m.TotalQuantity,
                UniqueItems = m.UniqueItems,
                UniqueUsers = m.UniqueUsers
            }).ToList();

            _reportData = reportData.Cast<object>().ToList();
            ReportDataGrid.ItemsSource = _reportData;

            AddDataGridColumn("Month", "Month");
            AddDataGridColumn("Assignment Count", "Assignments");
            AddDataGridColumn("Total Quantity", "TotalQuantity");
            AddDataGridColumn("Unique Items", "UniqueItems");
            AddDataGridColumn("Unique Users", "UniqueUsers");

            // Summary
            SummaryPanel.Visibility = Visibility.Visible;
            SummaryContent.Children.Clear();
            AddSummaryItem("Months Analyzed", trend.MonthsAnalyzed.ToString());
            AddSummaryItem("Average Assignments/Month", $"{trend.AverageAssignmentsPerMonth:F2}");
            AddSummaryItem("Average Quantity/Month", $"{trend.AverageQuantityPerMonth:F2}");
            AddSummaryItem("Highest Assignment Month", trend.HighestAssignmentMonth);
        }

        private void AddDataGridColumn(string header, string binding, string format = "")
        {
            var column = new DataGridTextColumn
            {
                Header = header,
                Binding = new System.Windows.Data.Binding(binding)
                {
                    StringFormat = format
                }
            };
            ReportDataGrid.Columns.Add(column);
        }

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            if (_reportData == null || _reportData.Count == 0)
            {
                MessageBox.Show("Please generate a report first.", "No Data",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"{_reportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExportDataToCSV(saveFileDialog.FileName);
                    MessageBox.Show("Report exported successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting report: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportDataToCSV(string filePath)
        {
            var csv = new StringBuilder();

            // Add header
            var headers = ReportDataGrid.Columns.Select(c => c.Header.ToString());
            csv.AppendLine(string.Join(",", headers));

            // Add data rows
            foreach (var item in _reportData)
            {
                var type = item.GetType();
                var values = ReportDataGrid.Columns.Select(column =>
                {
                    var binding = (column as DataGridTextColumn)?.Binding as System.Windows.Data.Binding;
                    if (binding != null)
                    {
                        var property = type.GetProperty(binding.Path.Path);
                        var value = property?.GetValue(item)?.ToString() ?? "";
                        return $"\"{value}\"";
                    }
                    return "";
                });
                csv.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(ReportDataGrid, ReportTitle.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
