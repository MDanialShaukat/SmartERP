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
    public partial class CustomerPage : Page
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authService;
        private List<Customer> _allCustomers;
        private Customer? _selectedCustomer;

        public CustomerPage(IUnitOfWork unitOfWork, IAuthenticationService authService)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _authService = authService;
            _allCustomers = new List<Customer>();
            
            Loaded += CustomerPage_Loaded;
        }

        private async void CustomerPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomerDataAsync();
        }

        private async System.Threading.Tasks.Task LoadCustomerDataAsync()
        {
            try
            {
                StatusText.Text = "Loading customer data...";
                
                var customerList = await _unitOfWork.Customers.GetAllAsync();
                _allCustomers = customerList.OrderByDescending(c => c.CreatedDate).ToList();
                
                CustomerDataGrid.ItemsSource = _allCustomers;
                UpdateRecordCount();
                
                StatusText.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading data";
            }
        }

        private void UpdateRecordCount()
        {
            var activeCount = _allCustomers.Count(c => c.IsActive);
            var totalCount = CustomerDataGrid.Items.Count;
            RecordCountText.Text = $"Total Records: {totalCount} (Active: {activeCount})";
        }

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCustomer = CustomerDataGrid.SelectedItem as Customer;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomerDialog(_unitOfWork, _authService.CurrentUser!.Id);
            
            if (dialog.ShowDialog() == true && dialog.CustomerData != null)
            {
                try
                {
                    StatusText.Text = "Adding customer...";
                    
                    // Check if customer code already exists
                    var existingCustomer = await _unitOfWork.Customers
                        .GetByCustomerCodeAsync(dialog.CustomerData.CustomerCode);
                    
                    if (existingCustomer != null)
                    {
                        MessageBox.Show("Customer code already exists. Please use a unique code.", 
                            "Duplicate Code", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StatusText.Text = "Ready";
                        return;
                    }
                    
                    await _unitOfWork.Customers.AddAsync(dialog.CustomerData);
                    await LoadCustomerDataAsync();
                    
                    MessageBox.Show("Customer added successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Customer added successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding customer: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error adding customer";
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Please select a customer to edit.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check permissions
            if (!_authService.IsAdmin && !_authService.HasPermission("Customer.Update"))
            {
                MessageBox.Show("You don't have permission to edit customers.", "Access Denied", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new CustomerDialog(_unitOfWork, _authService.CurrentUser!.Id, _selectedCustomer);
            
            if (dialog.ShowDialog() == true && dialog.CustomerData != null)
            {
                try
                {
                    StatusText.Text = "Updating customer...";
                    
                    await _unitOfWork.Customers.UpdateAsync(dialog.CustomerData);
                    await LoadCustomerDataAsync();
                    
                    MessageBox.Show("Customer updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Customer updated successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating customer: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error updating customer";
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Please select a customer to delete.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check permissions - only admin can delete
            if (!_authService.IsAdmin)
            {
                MessageBox.Show("Only administrators can delete customers.", "Access Denied", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedCustomer.CustomerName}'?\n\n" +
                $"Customer Code: {_selectedCustomer.CustomerCode}\n" +
                $"This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    StatusText.Text = "Deleting customer...";
                    
                    await _unitOfWork.Customers.DeleteAsync(_selectedCustomer);
                    await LoadCustomerDataAsync();
                    
                    MessageBox.Show("Customer deleted successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Customer deleted successfully";
                    _selectedCustomer = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error deleting customer";
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadCustomerDataAsync();
            _selectedCustomer = null;
            CustomerDataGrid.SelectedItem = null;
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
                CustomerDataGrid.ItemsSource = _allCustomers;
                UpdateRecordCount();
                StatusText.Text = "Ready";
                return;
            }

            try
            {
                var filtered = _allCustomers.Where(c =>
                    c.CustomerName.ToLower().Contains(searchText) ||
                    c.CustomerCode.ToLower().Contains(searchText) ||
                    c.PhoneNumber.ToLower().Contains(searchText) ||
                    c.Email.ToLower().Contains(searchText) ||
                    c.Area?.AreaName.ToLower().Contains(searchText) == true ||
                    c.Address.ToLower().Contains(searchText) ||
                    c.PackageType.ToLower().Contains(searchText)
                ).ToList();

                CustomerDataGrid.ItemsSource = filtered;
                UpdateRecordCount();
                StatusText.Text = $"Search: {filtered.Count} customer(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
