using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SmartERP.Core.Services;
using SmartERP.Data;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class BillingPage : Page
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authService;
        private List<Billing> _allBillings;
        private Billing? _selectedBilling;
        private List<Area> _allAreas;

        public BillingPage(IUnitOfWork unitOfWork, IAuthenticationService authService)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _authService = authService;
            _allBillings = new List<Billing>();
            _allAreas = new List<Area>();
            
            Loaded += BillingPage_Loaded;
        }

        private async void BillingPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAreasAsync();
            await LoadBillingDataAsync();
            InitializeFilters();
        }

        private async System.Threading.Tasks.Task LoadAreasAsync()
        {
            try
            {
                var areas = await _unitOfWork.Areas.GetActiveAreasAsync();
                _allAreas = areas.ToList();

                // Add "All Areas" option at the beginning
                AreaFilterComboBox.Items.Add(new Area { Id = 0, AreaName = "All Areas" });

                // Add all areas
                foreach (var area in _allAreas)
                {
                    AreaFilterComboBox.Items.Add(area);
                }

                // Select "All Areas" by default
                if (AreaFilterComboBox.Items.Count > 0)
                {
                    AreaFilterComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading areas: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeFilters()
        {
            // Populate year filter with last 5 years
            var currentYear = DateTime.Now.Year;
            YearFilterComboBox.Items.Add(new ComboBoxItem { Content = "All Years", IsSelected = true });
            
            for (int i = 0; i < 5; i++)
            {
                var year = currentYear - i;
                YearFilterComboBox.Items.Add(new ComboBoxItem { Content = year.ToString(), Tag = year });
            }
        }

        private async System.Threading.Tasks.Task LoadBillingDataAsync()
        {
            try
            {
                StatusText.Text = "Loading billing data...";
                
                var billingList = await _unitOfWork.Billings.GetAllAsync();
                _allBillings = billingList.OrderByDescending(b => b.BillDate).ToList();
                
                BillingDataGrid.ItemsSource = _allBillings;
                UpdateRecordCount();
                UpdateSummaryStats();
                
                StatusText.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading billings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading data";
            }
        }

        private void UpdateRecordCount()
        {
            var totalCount = BillingDataGrid.Items.Count;
            var paidCount = _allBillings.Count(b => b.PaymentStatus == "Paid");
            var pendingCount = _allBillings.Count(b => b.PaymentStatus == "Pending");
            RecordCountText.Text = $"Total: {totalCount} | Paid: {paidCount} | Pending: {pendingCount}";
        }

        private void UpdateSummaryStats()
        {
            var currentData = BillingDataGrid.ItemsSource as IEnumerable<Billing> ?? _allBillings;
            
            var totalPaid = currentData.Where(b => b.PaymentStatus == "Paid").Sum(b => b.AmountPaid);
            var totalPending = currentData.Where(b => b.PaymentStatus == "Pending" || b.PaymentStatus == "Partial")
                .Sum(b => b.BalanceAmount);
            var totalOverdue = currentData.Where(b => b.PaymentStatus == "Overdue")
                .Sum(b => b.BalanceAmount);

            TotalPaidText.Text = $"Rs. {totalPaid:N2}";
            TotalPendingText.Text = $"Rs. {totalPending:N2}";
            TotalOverdueText.Text = $"Rs. {totalOverdue:N2}";
        }

        private void BillingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBilling = BillingDataGrid.SelectedItem as Billing;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BillingDialog(_unitOfWork, _authService.CurrentUser!.Id);
            
            if (dialog.ShowDialog() == true && dialog.BillingData != null)
            {
                try
                {
                    StatusText.Text = "Adding billing...";
                    
                    // Check if bill number already exists
                    var existingBill = await _unitOfWork.Billings
                        .GetByBillNumberAsync(dialog.BillingData.BillNumber);
                    
                    if (existingBill != null)
                    {
                        MessageBox.Show("Bill number already exists. Please use a unique bill number.", 
                            "Duplicate Bill Number", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StatusText.Text = "Ready";
                        return;
                    }
                    
                    await _unitOfWork.Billings.AddAsync(dialog.BillingData);
                    await LoadBillingDataAsync();
                    
                    MessageBox.Show("Billing added successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Billing added successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding billing: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error adding billing";
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBilling == null)
            {
                MessageBox.Show("Please select a billing to edit.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check permissions
            if (!_authService.IsAdmin && !_authService.HasPermission("Billing.Update"))
            {
                MessageBox.Show("You don't have permission to edit billings.", "Access Denied", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new BillingDialog(_unitOfWork, _authService.CurrentUser!.Id, _selectedBilling);
            
            if (dialog.ShowDialog() == true && dialog.BillingData != null)
            {
                try
                {
                    StatusText.Text = "Updating billing...";
                    
                    await _unitOfWork.Billings.UpdateAsync(dialog.BillingData);
                    await LoadBillingDataAsync();
                    
                    MessageBox.Show("Billing updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Billing updated successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating billing: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error updating billing";
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBilling == null)
            {
                MessageBox.Show("Please select a billing to delete.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check permissions - only admin can delete
            if (!_authService.IsAdmin)
            {
                MessageBox.Show("Only administrators can delete billings.", "Access Denied", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete bill '{_selectedBilling.BillNumber}'?\n\n" +
                $"Customer: {_selectedBilling.Customer?.CustomerName}\n" +
                $"Amount: Rs. {_selectedBilling.TotalAmount:N2}\n\n" +
                $"This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    StatusText.Text = "Deleting billing...";
                    
                    await _unitOfWork.Billings.DeleteAsync(_selectedBilling);
                    await LoadBillingDataAsync();
                    
                    MessageBox.Show("Billing deleted successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Billing deleted successfully";
                    _selectedBilling = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting billing: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error deleting billing";
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadBillingDataAsync();
            _selectedBilling = null;
            BillingDataGrid.SelectedItem = null;
            
            // Reset filters
            StatusFilterComboBox.SelectedIndex = 0;
            MonthFilterComboBox.SelectedIndex = 0;
            YearFilterComboBox.SelectedIndex = 0;
            AreaFilterComboBox.SelectedIndex = 0;
            SearchTextBox.Clear();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            var searchText = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                BillingDataGrid.ItemsSource = _allBillings;
                UpdateRecordCount();
                UpdateSummaryStats();
                StatusText.Text = "Ready";
                return;
            }

            try
            {
                var filtered = _allBillings.Where(b =>
                    b.BillNumber.ToLower().Contains(searchText) ||
                    (b.Customer?.CustomerName.ToLower().Contains(searchText) ?? false) ||
                    (b.Customer?.CustomerCode.ToLower().Contains(searchText) ?? false) ||
                    b.PaymentStatus.ToLower().Contains(searchText) ||
                    b.PaymentMethod.ToLower().Contains(searchText) ||
                    b.TransactionReference.ToLower().Contains(searchText)
                ).ToList();

                BillingDataGrid.ItemsSource = filtered;
                UpdateRecordCount();
                UpdateSummaryStats();
                StatusText.Text = $"Search: {filtered.Count} billing(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Filter will be applied when Apply Filters button is clicked
        }

        private void MonthFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Filter will be applied when Apply Filters button is clicked
        }

        private void YearFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Filter will be applied when Apply Filters button is clicked
        }

        private void AreaFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Filter will be applied when Apply Filters button is clicked
        }

        private void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filtered = _allBillings.AsEnumerable();

                // Status filter
                var statusItem = StatusFilterComboBox.SelectedItem as ComboBoxItem;
                if (statusItem != null && statusItem.Content.ToString() != "All Bills")
                {
                    var status = statusItem.Content.ToString();
                    filtered = filtered.Where(b => b.PaymentStatus == status);
                }

                // Month filter
                var monthItem = MonthFilterComboBox.SelectedItem as ComboBoxItem;
                if (monthItem != null && monthItem.Tag != null)
                {
                    var month = int.Parse(monthItem.Tag.ToString()!);
                    filtered = filtered.Where(b => b.BillingMonth == month);
                }

                // Year filter
                var yearItem = YearFilterComboBox.SelectedItem as ComboBoxItem;
                if (yearItem != null && yearItem.Tag != null)
                {
                    var year = int.Parse(yearItem.Tag.ToString()!);
                    filtered = filtered.Where(b => b.BillingYear == year);
                }

                // Area filter
                var areaItem = AreaFilterComboBox.SelectedItem as Area;
                if (areaItem != null && areaItem.Id > 0)
                {
                    filtered = filtered.Where(b => b.Customer?.AreaId == areaItem.Id);
                }

                var result = filtered.ToList();
                BillingDataGrid.ItemsSource = result;
                UpdateRecordCount();
                UpdateSummaryStats();
                StatusText.Text = $"Filters applied: {result.Count} billing(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
