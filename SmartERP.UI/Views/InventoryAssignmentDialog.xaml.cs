using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SmartERP.Data;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class InventoryAssignmentDialog : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _currentUserId;
        private Inventory? _inventory;
        private List<User> _users;

        public InventoryAssignment? InventoryAssignment { get; private set; }

        public InventoryAssignmentDialog(IUnitOfWork unitOfWork, Inventory inventory, int currentUserId)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _inventory = inventory;
            _currentUserId = currentUserId;
            _users = new List<User>();

            Loaded += InventoryAssignmentDialog_Loaded;
        }

        private async void InventoryAssignmentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load all users for the combo box
                var users = await _unitOfWork.Users.GetAllAsync();
                _users = users.ToList();
                
                AssignToUserComboBox.ItemsSource = _users;

                // Populate inventory details
                if (_inventory != null)
                {
                    ItemNameTextBox.Text = _inventory.ItemName;
                    AvailableQuantityTextBox.Text = _inventory.QuantityRemaining.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuantityToAssignTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            if (_inventory == null)
                return;

            if (int.TryParse(QuantityToAssignTextBox.Text, out int quantityToAssign))
            {
                int remaining = _inventory.QuantityRemaining - quantityToAssign;
                
                SummaryQuantityTextBlock.Text = quantityToAssign.ToString();
                SummaryRemainingTextBlock.Text = remaining.ToString();

                // Change color if invalid
                if (remaining < 0)
                {
                    SummaryRemainingTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    SummaryRemainingTextBlock.Foreground = System.Windows.Media.Brushes.DarkRed;
                }
            }
            else
            {
                SummaryQuantityTextBlock.Text = "0";
                SummaryRemainingTextBlock.Text = _inventory.QuantityRemaining.ToString();
            }
        }

        private async void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            if (_inventory == null)
            {
                MessageBox.Show("Inventory item is not selected.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AssignToUserComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to assign the inventory to.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityToAssignTextBox.Text, out int quantityToAssign) || quantityToAssign <= 0)
            {
                MessageBox.Show("Please enter a valid quantity greater than zero.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedUser = AssignToUserComboBox.SelectedItem as User;
                
                // Call the assignment method from the repository
                InventoryAssignment = await _unitOfWork.Inventories.AssignInventoryAsync(
                    inventoryId: _inventory.Id,
                    userId: selectedUser!.Id,
                    quantityAssigned: quantityToAssign,
                    remarks: RemarksTextBox.Text,
                    createdBy: _currentUserId
                );

                MessageBox.Show($"Successfully assigned {quantityToAssign} item(s) to {selectedUser.FullName}!", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Assignment Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning inventory: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
