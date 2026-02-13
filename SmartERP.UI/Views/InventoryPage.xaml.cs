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
    public partial class InventoryPage : Page
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authService;
        private List<Inventory> _allInventory;
        private Inventory? _selectedInventory;

        public InventoryPage(IUnitOfWork unitOfWork, IAuthenticationService authService)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _authService = authService;
            _allInventory = new List<Inventory>();
            
            Loaded += InventoryPage_Loaded;
        }

        private async void InventoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInventoryDataAsync();
        }

        private async System.Threading.Tasks.Task LoadInventoryDataAsync()
        {
            try
            {
                StatusText.Text = "Loading inventory data...";
                
                var inventoryList = await _unitOfWork.Inventories.GetAllAsync();
                _allInventory = inventoryList.OrderByDescending(i => i.CreatedDate).ToList();
                
                InventoryDataGrid.ItemsSource = _allInventory;
                UpdateRecordCount();
                
                StatusText.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading data";
            }
        }

        private void UpdateRecordCount()
        {
            RecordCountText.Text = $"Total Records: {InventoryDataGrid.Items.Count}";
        }

        private void InventoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedInventory = InventoryDataGrid.SelectedItem as Inventory;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InventoryDialog(_authService.CurrentUser!.Id);
            
            if (dialog.ShowDialog() == true && dialog.InventoryItem != null)
            {
                try
                {
                    StatusText.Text = "Adding inventory item...";
                    
                    await _unitOfWork.Inventories.AddAsync(dialog.InventoryItem);
                    await LoadInventoryDataAsync();
                    
                    MessageBox.Show("Inventory item added successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Item added successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding inventory item: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error adding item";
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInventory == null)
            {
                MessageBox.Show("Please select an inventory item to edit.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check permissions
            if (!_authService.IsAdmin && !_authService.HasPermission("Inventory.Update"))
            {
                MessageBox.Show("You don't have permission to edit inventory items.", "Access Denied", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new InventoryDialog(_authService.CurrentUser!.Id, _selectedInventory);
            
            if (dialog.ShowDialog() == true && dialog.InventoryItem != null)
            {
                try
                {
                    StatusText.Text = "Updating inventory item...";
                    
                    await _unitOfWork.Inventories.UpdateAsync(dialog.InventoryItem);
                    await LoadInventoryDataAsync();
                    
                    MessageBox.Show("Inventory item updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Item updated successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating inventory item: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error updating item";
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInventory == null)
            {
                MessageBox.Show("Please select an inventory item to delete.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check permissions - only admin can delete
            if (!_authService.IsAdmin)
            {
                MessageBox.Show("Only administrators can delete inventory items.", "Access Denied", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedInventory.ItemName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    StatusText.Text = "Deleting inventory item...";
                    
                    await _unitOfWork.Inventories.DeleteAsync(_selectedInventory);
                    await LoadInventoryDataAsync();
                    
                    MessageBox.Show("Inventory item deleted successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Item deleted successfully";
                    _selectedInventory = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting inventory item: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error deleting item";
                }
            }
        }

        private async void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInventory == null)
            {
                MessageBox.Show("Please select an inventory item to assign.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_selectedInventory.QuantityRemaining <= 0)
            {
                MessageBox.Show($"No inventory available for assignment. Available quantity: {_selectedInventory.QuantityRemaining}", 
                    "Insufficient Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new InventoryAssignmentDialog(_unitOfWork, _selectedInventory, _authService.CurrentUser!.Id);
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    StatusText.Text = "Assignment completed successfully";
                    await LoadInventoryDataAsync();
                    
                    MessageBox.Show("Inventory assigned successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during assignment: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error during assignment";
                }
            }
        }

        private async void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInventory == null)
            {
                MessageBox.Show("Please select an inventory item to view its assignment history.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new InventoryAssignmentHistoryDialog(_unitOfWork, _selectedInventory);
            dialog.ShowDialog();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadInventoryDataAsync();
            _selectedInventory = null;
            InventoryDataGrid.SelectedItem = null;
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
                InventoryDataGrid.ItemsSource = _allInventory;
                UpdateRecordCount();
                StatusText.Text = "Ready";
                return;
            }

            try
            {
                var filtered = _allInventory.Where(i =>
                    i.ItemName.ToLower().Contains(searchText) ||
                    i.Category.ToLower().Contains(searchText) ||
                    i.Supplier.ToLower().Contains(searchText) ||
                    i.Description.ToLower().Contains(searchText) ||
                    i.Unit.ToLower().Contains(searchText)
                ).ToList();

                InventoryDataGrid.ItemsSource = filtered;
                UpdateRecordCount();
                StatusText.Text = $"Search: {filtered.Count} item(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
