using System;
using System.Windows;
using SmartERP.Data;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class AddRecoveryPersonDialog : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private int _currentUserId;
        public RecoveryPerson? CreatedRecoveryPerson { get; private set; }

        public AddRecoveryPersonDialog(IUnitOfWork unitOfWork, int currentUserId)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _currentUserId = currentUserId;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            SaveAsync();
        }

        private async void SaveAsync()
        {
            try
            {
                var recoveryPerson = new RecoveryPerson
                {
                    PersonName = PersonNameTextBox.Text.Trim(),
                    PhoneNumber = PhoneNumberTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Address = AddressTextBox.Text.Trim(),
                    Notes = NotesTextBox.Text.Trim(),
                    IsActive = true,
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.Now
                };

                // Save to database
                await _unitOfWork.RecoveryPersons.AddAsync(recoveryPerson);
                await _unitOfWork.SaveChangesAsync();

                CreatedRecoveryPerson = recoveryPerson;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving recovery person: {ex.Message}", "Error",
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
            // Person Name
            if (string.IsNullOrWhiteSpace(PersonNameTextBox.Text))
            {
                MessageBox.Show("Please enter person name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PersonNameTextBox.Focus();
                return false;
            }

            return true;
        }
    }
}
