using System;
using System.Windows;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class CustomerDialog : Window
    {
        public Customer? CustomerData { get; private set; }
        private bool _isEditMode;
        private int _currentUserId;

        public CustomerDialog(int currentUserId, Customer? existingCustomer = null)
        {
            InitializeComponent();
            _currentUserId = currentUserId;

            if (existingCustomer != null)
            {
                _isEditMode = true;
                CustomerData = existingCustomer;
                LoadCustomerData(existingCustomer);
                DialogTitle.Text = "Edit Customer";
                CustomerCodeTextBox.IsReadOnly = true; // Don't allow changing customer code
                CustomerCodeTextBox.Background = System.Windows.Media.Brushes.LightGray;
            }
            else
            {
                _isEditMode = false;
                CustomerData = new Customer();
                ConnectionDatePicker.SelectedDate = DateTime.Now;
                PackageAmountTextBox.Text = "0";
                OutstandingBalanceTextBox.Text = "0";
            }
        }

        private void LoadCustomerData(Customer customer)
        {
            CustomerCodeTextBox.Text = customer.CustomerCode;
            CustomerNameTextBox.Text = customer.CustomerName;
            PhoneNumberTextBox.Text = customer.PhoneNumber;
            EmailTextBox.Text = customer.Email;
            AddressTextBox.Text = customer.Address;
            CityTextBox.Text = customer.City;
            PinCodeTextBox.Text = customer.PinCode;
            PackageTypeComboBox.Text = customer.PackageType;
            PackageAmountTextBox.Text = customer.PackageAmount.ToString("F2");
            ConnectionTypeComboBox.Text = customer.ConnectionType;
            ConnectionDatePicker.SelectedDate = customer.ConnectionDate;
            OutstandingBalanceTextBox.Text = customer.OutstandingBalance.ToString("F2");
            IsActiveCheckBox.IsChecked = customer.IsActive;
            AdditionalDetailsTextBox.Text = customer.AdditionalDetails;
            NotesTextBox.Text = customer.Notes;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Update customer data
                CustomerData!.CustomerCode = CustomerCodeTextBox.Text.Trim();
                CustomerData.CustomerName = CustomerNameTextBox.Text.Trim();
                CustomerData.PhoneNumber = PhoneNumberTextBox.Text.Trim();
                CustomerData.Email = EmailTextBox.Text.Trim();
                CustomerData.Address = AddressTextBox.Text.Trim();
                CustomerData.City = CityTextBox.Text.Trim();
                CustomerData.PinCode = PinCodeTextBox.Text.Trim();
                CustomerData.PackageType = PackageTypeComboBox.Text.Trim();
                CustomerData.PackageAmount = decimal.Parse(PackageAmountTextBox.Text);
                CustomerData.ConnectionType = ConnectionTypeComboBox.Text.Trim();
                CustomerData.ConnectionDate = ConnectionDatePicker.SelectedDate ?? DateTime.Now;
                CustomerData.OutstandingBalance = decimal.Parse(OutstandingBalanceTextBox.Text);
                CustomerData.IsActive = IsActiveCheckBox.IsChecked ?? true;
                CustomerData.AdditionalDetails = AdditionalDetailsTextBox.Text.Trim();
                CustomerData.Notes = NotesTextBox.Text.Trim();

                if (!_isEditMode)
                {
                    CustomerData.CreatedBy = _currentUserId;
                    CustomerData.CreatedDate = DateTime.Now;
                }
                else
                {
                    CustomerData.LastModifiedBy = _currentUserId;
                    CustomerData.LastModifiedDate = DateTime.Now;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving customer: {ex.Message}", "Error", 
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
            // Customer Code
            if (string.IsNullOrWhiteSpace(CustomerCodeTextBox.Text))
            {
                MessageBox.Show("Please enter customer code.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomerCodeTextBox.Focus();
                return false;
            }

            // Customer Name
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                MessageBox.Show("Please enter customer name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomerNameTextBox.Focus();
                return false;
            }

            // Phone Number
            if (string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text))
            {
                MessageBox.Show("Please enter phone number.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PhoneNumberTextBox.Focus();
                return false;
            }

            // Address
            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Please enter address.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                AddressTextBox.Focus();
                return false;
            }

            // Package Amount
            if (!decimal.TryParse(PackageAmountTextBox.Text, out decimal packageAmount) || packageAmount < 0)
            {
                MessageBox.Show("Please enter a valid package amount (0 or greater).", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PackageAmountTextBox.Focus();
                return false;
            }

            // Outstanding Balance
            if (!decimal.TryParse(OutstandingBalanceTextBox.Text, out decimal outstandingBalance) || outstandingBalance < 0)
            {
                MessageBox.Show("Please enter a valid outstanding balance (0 or greater).", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                OutstandingBalanceTextBox.Focus();
                return false;
            }

            // Connection Date
            if (!ConnectionDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a connection date.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ConnectionDatePicker.Focus();
                return false;
            }

            return true;
        }
    }
}
