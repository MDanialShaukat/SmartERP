using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SmartERP.Data;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class BillingDialog : Window
    {
        public Billing? BillingData { get; private set; }
        private readonly IUnitOfWork _unitOfWork;
        private bool _isEditMode;
        private int _currentUserId;
        private List<Customer> _customers;
        private List<Area> _areas;

        public BillingDialog(IUnitOfWork unitOfWork, int currentUserId, Billing? existingBilling = null)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _currentUserId = currentUserId;
            _customers = new List<Customer>();
            _areas = new List<Area>();

            Loaded += BillingDialog_Loaded;

            if (existingBilling != null)
            {
                _isEditMode = true;
                BillingData = existingBilling;
                DialogTitle.Text = "Edit Billing";
                BillNumberTextBox.IsReadOnly = true;
                BillNumberTextBox.Background = System.Windows.Media.Brushes.LightGray;
            }
            else
            {
                _isEditMode = false;
                BillingData = new Billing();
                BillDatePicker.SelectedDate = DateTime.Now;
                DueDatePicker.SelectedDate = DateTime.Now.AddDays(15);
                BillingMonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            }

            InitializeYearComboBox();
        }

        private void InitializeYearComboBox()
        {
            var currentYear = DateTime.Now.Year;
            for (int i = 0; i < 5; i++)
            {
                var year = currentYear - i;
                BillingYearComboBox.Items.Add(new ComboBoxItem { Content = year.ToString(), Tag = year });
            }
            BillingYearComboBox.SelectedIndex = 0;
        }

        private async void BillingDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAreasAsync();
            await LoadCustomersAsync();

            if (_isEditMode && BillingData != null)
            {
                LoadBillingData(BillingData);
            }
        }

        private async System.Threading.Tasks.Task LoadAreasAsync()
        {
            try
            {
                var areas = await _unitOfWork.Areas.GetActiveAreasAsync();
                _areas = areas.ToList();
                AreaComboBox.ItemsSource = _areas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading areas: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                _customers = customers.Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToList();
                
                // If area is selected, filter customers by area
                if (AreaComboBox.SelectedItem is Area selectedArea)
                {
                    var filteredCustomers = _customers.Where(c => c.AreaId == selectedArea.Id).ToList();
                    CustomerComboBox.ItemsSource = filteredCustomers;
                }
                else
                {
                    // Show all customers if no area selected
                    CustomerComboBox.ItemsSource = _customers;
                }

                if (_customers.Any())
                {
                    if (CustomerComboBox.Items.Count > 0)
                    {
                        CustomerComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBillingData(Billing billing)
        {
            BillNumberTextBox.Text = billing.BillNumber;
            
            // Select area
            if (billing.Customer?.Area != null)
            {
                var area = _areas.FirstOrDefault(a => a.Id == billing.Customer.Area.Id);
                if (area != null)
                {
                    AreaComboBox.SelectedItem = area;
                }
            }
            
            // Select customer
            var customer = _customers.FirstOrDefault(c => c.Id == billing.CustomerId);
            if (customer != null)
            {
                CustomerComboBox.SelectedItem = customer;
            }

            BillingMonthComboBox.SelectedIndex = billing.BillingMonth - 1;
            
            // Select year
            var yearItem = BillingYearComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => (int)item.Tag == billing.BillingYear);
            if (yearItem != null)
            {
                BillingYearComboBox.SelectedItem = yearItem;
            }

            BillDatePicker.SelectedDate = billing.BillDate;
            DueDatePicker.SelectedDate = billing.DueDate;
            BillAmountTextBox.Text = billing.BillAmount.ToString("F2");
            PreviousDueTextBox.Text = billing.PreviousDue.ToString("F2");
            AmountPaidTextBox.Text = billing.AmountPaid.ToString("F2");
            
            PaymentStatusComboBox.Text = billing.PaymentStatus;
            PaymentMethodComboBox.Text = billing.PaymentMethod;
            TransactionReferenceTextBox.Text = billing.TransactionReference;
            PaymentDatePicker.SelectedDate = billing.PaymentDate;
            NotesTextBox.Text = billing.Notes;

            // Display audit information
            if (AuditInfoPanel != null)
            {
                AuditInfoPanel.Visibility = System.Windows.Visibility.Visible;
                
                CreatedByText.Text = billing.CreatedByUser?.FullName ?? "Unknown";
                CreatedDateText.Text = billing.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                
                if (billing.LastModifiedBy.HasValue && billing.LastModifiedByUser != null)
                {
                    LastModifiedByText.Text = billing.LastModifiedByUser.FullName;
                    LastModifiedDateText.Text = billing.LastModifiedDate?.ToString("dd/MM/yyyy HH:mm:ss") ?? "-";
                }
                else
                {
                    LastModifiedByText.Text = "Not modified";
                    LastModifiedDateText.Text = "-";
                }
            }

            CalculateTotals(null, null);
        }

        private void CustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerComboBox?.SelectedItem is Customer customer && !_isEditMode)
            {
                // Auto-fill previous due from customer's outstanding balance
                if (PreviousDueTextBox != null)
                {
                    PreviousDueTextBox.Text = customer.OutstandingBalance.ToString("F2");
                }
                
                // Auto-fill bill amount from customer's package amount
                if (customer.PackageAmount > 0 && BillAmountTextBox != null)
                {
                    BillAmountTextBox.Text = customer.PackageAmount.ToString("F2");
                }
            }
        }

        private async void AreaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AreaComboBox.SelectedItem is Area selectedArea && !_isEditMode)
            {
                // Filter customers by selected area
                var customersInArea = _customers.Where(c => c.AreaId == selectedArea.Id).OrderBy(c => c.CustomerName).ToList();
                CustomerComboBox.ItemsSource = customersInArea;

                if (customersInArea.Any())
                {
                    CustomerComboBox.SelectedIndex = 0;
                }
                else
                {
                    CustomerComboBox.SelectedItem = null;
                    MessageBox.Show($"No customers found in {selectedArea.AreaName} area.", "No Customers",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CustomerSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PerformCustomerSearch();
        }

        private void PerformCustomerSearch()
        {
            var searchText = CustomerSearchBox.Text.Trim().ToLower();

            // Get base list to search from
            List<Customer> baseList = _customers;
            if (AreaComboBox.SelectedItem is Area selectedArea && selectedArea.Id > 0)
            {
                baseList = _customers.Where(c => c.AreaId == selectedArea.Id).ToList();
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Show all customers in area (or all if no area selected)
                CustomerComboBox.ItemsSource = baseList.OrderBy(c => c.CustomerName).ToList();
                if (baseList.Any())
                {
                    CustomerComboBox.SelectedIndex = 0;
                }
                return;
            }

            try
            {
                // Search by customer name or customer code
                var filtered = baseList.Where(c =>
                    c.CustomerName.ToLower().Contains(searchText) ||
                    c.CustomerCode.ToLower().Contains(searchText)
                ).OrderBy(c => c.CustomerName).ToList();

                if (filtered.Any())
                {
                    CustomerComboBox.ItemsSource = filtered;
                    CustomerComboBox.SelectedIndex = 0;
                }
                else
                {
                    CustomerComboBox.ItemsSource = new List<Customer>();
                    CustomerComboBox.SelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearCustomerSearchButton_Click(object sender, RoutedEventArgs e)
        {
            CustomerSearchBox.Text = "";
            PerformCustomerSearch();
        }

        private void ClearAreaButton_Click(object sender, RoutedEventArgs e)
        {
            AreaComboBox.SelectedItem = null;
            CustomerSearchBox.Text = "";
            
            // Show all customers again
            CustomerComboBox.ItemsSource = _customers.OrderBy(c => c.CustomerName).ToList();
            if (_customers.Any())
            {
                CustomerComboBox.SelectedIndex = 0;
            }
        }

        private async void GenerateBillNumberButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var billNumber = await _unitOfWork.Billings.GenerateBillNumberAsync();
                BillNumberTextBox.Text = billNumber;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating bill number: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotals(object? sender, TextChangedEventArgs? e)
        {
            // Check if controls are initialized
            if (BillAmountTextBox == null || PreviousDueTextBox == null || 
                AmountPaidTextBox == null || TotalAmountText == null || 
                BalanceAmountText == null || PaymentStatusComboBox == null ||
                DueDatePicker == null)
            {
                return;
            }

            try
            {
                decimal billAmount = 0;
                decimal previousDue = 0;
                decimal amountPaid = 0;

                if (!string.IsNullOrEmpty(BillAmountTextBox.Text) && 
                    decimal.TryParse(BillAmountTextBox.Text, out var ba))
                    billAmount = ba;

                if (!string.IsNullOrEmpty(PreviousDueTextBox.Text) && 
                    decimal.TryParse(PreviousDueTextBox.Text, out var pd))
                    previousDue = pd;

                if (!string.IsNullOrEmpty(AmountPaidTextBox.Text) && 
                    decimal.TryParse(AmountPaidTextBox.Text, out var ap))
                    amountPaid = ap;

                var totalAmount = billAmount + previousDue;
                var balanceAmount = totalAmount - amountPaid;

                TotalAmountText.Text = $"Rs. {totalAmount:N2}";
                BalanceAmountText.Text = $"Rs. {balanceAmount:N2}";

                // Auto-update payment status based on balance
                if (balanceAmount <= 0 && amountPaid > 0)
                {
                    PaymentStatusComboBox.Text = "Paid";
                }
                else if (amountPaid > 0 && balanceAmount > 0)
                {
                    PaymentStatusComboBox.Text = "Partial";
                }
                else if (DueDatePicker.SelectedDate.HasValue && 
                         DueDatePicker.SelectedDate.Value < DateTime.Now.Date)
                {
                    PaymentStatusComboBox.Text = "Overdue";
                }
                else
                {
                    PaymentStatusComboBox.Text = "Pending";
                }
            }
            catch
            {
                // Ignore calculation errors during input
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Update billing data
                BillingData!.BillNumber = BillNumberTextBox.Text.Trim();
                BillingData.CustomerId = ((Customer)CustomerComboBox.SelectedItem).Id;
                
                var monthItem = (ComboBoxItem)BillingMonthComboBox.SelectedItem;
                BillingData.BillingMonth = int.Parse(monthItem.Tag.ToString()!);
                
                var yearItem = (ComboBoxItem)BillingYearComboBox.SelectedItem;
                BillingData.BillingYear = int.Parse(yearItem.Tag.ToString()!);
                
                BillingData.BillDate = BillDatePicker.SelectedDate ?? DateTime.Now;
                BillingData.DueDate = DueDatePicker.SelectedDate ?? DateTime.Now.AddDays(15);
                BillingData.BillAmount = decimal.Parse(BillAmountTextBox.Text);
                BillingData.PreviousDue = decimal.Parse(PreviousDueTextBox.Text);
                BillingData.TotalAmount = BillingData.BillAmount + BillingData.PreviousDue;
                BillingData.AmountPaid = decimal.Parse(AmountPaidTextBox.Text);
                BillingData.BalanceAmount = BillingData.TotalAmount - BillingData.AmountPaid;
                BillingData.PaymentStatus = PaymentStatusComboBox.Text;
                BillingData.PaymentMethod = PaymentMethodComboBox.Text.Trim();
                BillingData.TransactionReference = TransactionReferenceTextBox.Text.Trim();
                BillingData.PaymentDate = PaymentDatePicker.SelectedDate;
                BillingData.Notes = NotesTextBox.Text.Trim();

                if (!_isEditMode)
                {
                    BillingData.CreatedBy = _currentUserId;
                    BillingData.CreatedDate = DateTime.Now;
                }
                else
                {
                    BillingData.LastModifiedBy = _currentUserId;
                    BillingData.LastModifiedDate = DateTime.Now;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving billing: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            // Bill Number
            if (string.IsNullOrWhiteSpace(BillNumberTextBox.Text))
            {
                MessageBox.Show("Please enter or generate a bill number.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                BillNumberTextBox.Focus();
                return false;
            }

            // Customer
            if (CustomerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomerComboBox.Focus();
                return false;
            }

            // Billing Month
            if (BillingMonthComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a billing month.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                BillingMonthComboBox.Focus();
                return false;
            }

            // Billing Year
            if (BillingYearComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a billing year.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                BillingYearComboBox.Focus();
                return false;
            }

            // Bill Date
            if (!BillDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a bill date.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                BillDatePicker.Focus();
                return false;
            }

            // Due Date
            if (!DueDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a due date.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DueDatePicker.Focus();
                return false;
            }

            // Bill Amount
            if (!decimal.TryParse(BillAmountTextBox.Text, out decimal billAmount) || billAmount < 0)
            {
                MessageBox.Show("Please enter a valid bill amount (0 or greater).", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                BillAmountTextBox.Focus();
                return false;
            }

            // Previous Due
            if (!decimal.TryParse(PreviousDueTextBox.Text, out decimal previousDue) || previousDue < 0)
            {
                MessageBox.Show("Please enter a valid previous due amount (0 or greater).", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PreviousDueTextBox.Focus();
                return false;
            }

            // Amount Paid
            if (!decimal.TryParse(AmountPaidTextBox.Text, out decimal amountPaid) || amountPaid < 0)
            {
                MessageBox.Show("Please enter a valid amount paid (0 or greater).", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountPaidTextBox.Focus();
                return false;
            }

            // Check if amount paid exceeds total
            var totalAmount = billAmount + previousDue;
            if (amountPaid > totalAmount)
            {
                MessageBox.Show("Amount paid cannot exceed total amount.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountPaidTextBox.Focus();
                return false;
            }

            // Payment Status
            if (string.IsNullOrWhiteSpace(PaymentStatusComboBox.Text))
            {
                MessageBox.Show("Please select a payment status.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PaymentStatusComboBox.Focus();
                return false;
            }

            return true;
        }
    }
}
