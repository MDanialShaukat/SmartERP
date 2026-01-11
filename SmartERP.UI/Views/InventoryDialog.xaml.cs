using System;
using System.Windows;
using System.Windows.Controls;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class InventoryDialog : Window
    {
        public Inventory? InventoryItem { get; private set; }
        private bool _isEditMode;
        private int _currentUserId;

        public InventoryDialog(int currentUserId, Inventory? existingItem = null)
        {
            InitializeComponent();
            _currentUserId = currentUserId;

            if (existingItem != null)
            {
                _isEditMode = true;
                InventoryItem = existingItem;
                LoadInventoryData(existingItem);
                DialogTitle.Text = "Edit Inventory Item";
                QuantityGrid.Visibility = Visibility.Visible;
            }
            else
            {
                _isEditMode = false;
                InventoryItem = new Inventory();
                PurchaseDatePicker.SelectedDate = DateTime.Now;
            }
        }

        private void LoadInventoryData(Inventory item)
        {
            ItemNameTextBox.Text = item.ItemName;
            DescriptionTextBox.Text = item.Description;
            CategoryComboBox.Text = item.Category;
            UnitComboBox.Text = item.Unit;
            PurchasePriceTextBox.Text = item.PurchasePrice.ToString("F2");
            QuantityPurchasedTextBox.Text = item.QuantityPurchased.ToString();
            QuantityUsedTextBox.Text = item.QuantityUsed.ToString();
            QuantityRemainingTextBox.Text = item.QuantityRemaining.ToString();
            SupplierTextBox.Text = item.Supplier;
            PurchaseDatePicker.SelectedDate = item.PurchaseDate;
            NotesTextBox.Text = item.Notes;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Update inventory item
                InventoryItem!.ItemName = ItemNameTextBox.Text.Trim();
                InventoryItem.Description = DescriptionTextBox.Text.Trim();
                InventoryItem.Category = CategoryComboBox.Text.Trim();
                InventoryItem.Unit = UnitComboBox.Text.Trim();
                InventoryItem.PurchasePrice = decimal.Parse(PurchasePriceTextBox.Text);
                InventoryItem.QuantityPurchased = int.Parse(QuantityPurchasedTextBox.Text);
                
                if (!_isEditMode)
                {
                    InventoryItem.QuantityUsed = 0;
                    InventoryItem.QuantityRemaining = InventoryItem.QuantityPurchased;
                    InventoryItem.CreatedBy = _currentUserId;
                    InventoryItem.CreatedDate = DateTime.Now;
                }
                else
                {
                    // Recalculate remaining quantity
                    InventoryItem.QuantityRemaining = InventoryItem.QuantityPurchased - InventoryItem.QuantityUsed;
                    InventoryItem.LastModifiedBy = _currentUserId;
                    InventoryItem.LastModifiedDate = DateTime.Now;
                }

                InventoryItem.TotalPurchaseAmount = InventoryItem.PurchasePrice * InventoryItem.QuantityPurchased;
                InventoryItem.Supplier = SupplierTextBox.Text.Trim();
                InventoryItem.PurchaseDate = PurchaseDatePicker.SelectedDate ?? DateTime.Now;
                InventoryItem.Notes = NotesTextBox.Text.Trim();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving inventory item: {ex.Message}", "Error", 
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
            // Item Name
            if (string.IsNullOrWhiteSpace(ItemNameTextBox.Text))
            {
                MessageBox.Show("Please enter item name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ItemNameTextBox.Focus();
                return false;
            }

            // Category
            if (string.IsNullOrWhiteSpace(CategoryComboBox.Text))
            {
                MessageBox.Show("Please select or enter a category.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return false;
            }

            // Unit
            if (string.IsNullOrWhiteSpace(UnitComboBox.Text))
            {
                MessageBox.Show("Please select or enter a unit.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UnitComboBox.Focus();
                return false;
            }

            // Purchase Price
            if (!decimal.TryParse(PurchasePriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid purchase price (greater than 0).", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PurchasePriceTextBox.Focus();
                return false;
            }

            // Quantity Purchased
            if (!int.TryParse(QuantityPurchasedTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity (greater than 0).", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityPurchasedTextBox.Focus();
                return false;
            }

            // Purchase Date
            if (!PurchaseDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a purchase date.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PurchaseDatePicker.Focus();
                return false;
            }

            return true;
        }
    }
}
