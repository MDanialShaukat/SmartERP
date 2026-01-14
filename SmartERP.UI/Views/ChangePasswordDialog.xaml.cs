using System;
using System.Windows;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class ChangePasswordDialog : Window
    {
        public string NewPassword { get; private set; } = string.Empty;
        private readonly User _user;

        public ChangePasswordDialog(User user)
        {
            InitializeComponent();
            _user = user;
            UserInfoText.Text = $"User: {user.Username} ({user.FullName})";
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                NewPassword = NewPasswordBox.Password;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing password: {ex.Message}", "Error", 
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
            // New Password
            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                MessageBox.Show("Please enter new password.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return false;
            }

            if (NewPasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return false;
            }

            // Confirm Password
            if (string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Please confirm new password.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return false;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return false;
            }

            return true;
        }
    }
}
