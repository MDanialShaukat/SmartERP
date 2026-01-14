using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class UserDialog : Window
    {
        public User? UserData { get; private set; }
        private bool _isEditMode;
        private bool _isEditingOwnAccount;

        public UserDialog(User? existingUser = null, bool isEditingOwnAccount = false)
        {
            InitializeComponent();
            _isEditingOwnAccount = isEditingOwnAccount;

            if (existingUser != null)
            {
                _isEditMode = true;
                UserData = existingUser;
                LoadUserData(existingUser);
                DialogTitle.Text = "Edit User";
                UsernameTextBox.IsReadOnly = true;
                UsernameTextBox.Background = System.Windows.Media.Brushes.LightGray;
                PasswordPanel.Visibility = Visibility.Collapsed;
                InfoPanel.Visibility = Visibility.Visible;

                // If editing own account, disable role and active status
                if (_isEditingOwnAccount)
                {
                    DialogTitle.Text = "Edit My Profile";
                    RoleComboBox.IsEnabled = false;
                    RoleComboBox.Background = System.Windows.Media.Brushes.LightGray;
                    IsActiveCheckBox.IsEnabled = false;
                    
                    // Show warning about restrictions
                    InfoPanel.Background = System.Windows.Media.Brushes.LightYellow;
                    var infoTextBlock = (TextBlock)((StackPanel)InfoPanel.Child).Children[0];
                    infoTextBlock.Text = "Note: You cannot change your own role or deactivate your account. Contact another administrator if these changes are needed.";
                    infoTextBlock.Foreground = System.Windows.Media.Brushes.DarkOrange;
                }
            }
            else
            {
                _isEditMode = false;
                UserData = new User();
            }
        }

        private void LoadUserData(User user)
        {
            UsernameTextBox.Text = user.Username;
            FullNameTextBox.Text = user.FullName;
            EmailTextBox.Text = user.Email;
            PhoneNumberTextBox.Text = user.PhoneNumber;
            RoleComboBox.Text = user.Role;
            IsActiveCheckBox.IsChecked = user.IsActive;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Update user data
                UserData!.Username = UsernameTextBox.Text.Trim();
                UserData.FullName = FullNameTextBox.Text.Trim();
                UserData.Email = EmailTextBox.Text.Trim();
                UserData.PhoneNumber = PhoneNumberTextBox.Text.Trim();
                UserData.Role = ((System.Windows.Controls.ComboBoxItem)RoleComboBox.SelectedItem).Tag.ToString()!;
                UserData.IsActive = IsActiveCheckBox.IsChecked ?? true;

                if (!_isEditMode)
                {
                    // Hash password for new user
                    UserData.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password);
                    UserData.CreatedDate = DateTime.Now;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Error", 
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
            // Username
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter username.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            if (UsernameTextBox.Text.Length < 3)
            {
                MessageBox.Show("Username must be at least 3 characters long.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            // Full Name
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Please enter full name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameTextBox.Focus();
                return false;
            }

            // Email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Please enter email address.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Basic email validation
            if (!EmailTextBox.Text.Contains("@") || !EmailTextBox.Text.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Role
            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a role.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                RoleComboBox.Focus();
                return false;
            }

            // Password validation (only for new users)
            if (!_isEditMode)
            {
                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Please enter password.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PasswordBox.Focus();
                    return false;
                }

                if (PasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PasswordBox.Focus();
                    return false;
                }

                if (PasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Passwords do not match.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ConfirmPasswordBox.Focus();
                    return false;
                }
            }

            return true;
        }
    }
}
