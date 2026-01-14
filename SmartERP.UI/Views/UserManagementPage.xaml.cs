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
    public partial class UserManagementPage : Page
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authService;
        private List<User> _allUsers;
        private User? _selectedUser;

        public UserManagementPage(IUnitOfWork unitOfWork, IAuthenticationService authService)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _authService = authService;
            _allUsers = new List<User>();
            
            Loaded += UserManagementPage_Loaded;
        }

        private async void UserManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserDataAsync();
        }

        private async System.Threading.Tasks.Task LoadUserDataAsync()
        {
            try
            {
                StatusText.Text = "Loading user data...";
                
                var userList = await _unitOfWork.Users.GetAllAsync();
                _allUsers = userList.OrderByDescending(u => u.CreatedDate).ToList();
                
                UserDataGrid.ItemsSource = _allUsers;
                UpdateRecordCount();
                
                StatusText.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading data";
            }
        }

        private void UpdateRecordCount()
        {
            var activeCount = _allUsers.Count(u => u.IsActive);
            var adminCount = _allUsers.Count(u => u.Role == "Admin");
            var totalCount = UserDataGrid.Items.Count;
            RecordCountText.Text = $"Total: {totalCount} | Active: {activeCount} | Admins: {adminCount}";
        }

        private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = UserDataGrid.SelectedItem as User;
        }

        private void UserDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // Prevent any inline editing - all changes must go through the Edit dialog
            e.Cancel = true;
            
            // Show message if user tries to edit
            if (_selectedUser != null)
            {
                StatusText.Text = "Please use the 'Edit' button to modify user information";
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserDialog();
            
            if (dialog.ShowDialog() == true && dialog.UserData != null)
            {
                try
                {
                    StatusText.Text = "Adding user...";
                    
                    // Check if username already exists
                    var existingUser = await _unitOfWork.Users
                        .GetByUsernameAsync(dialog.UserData.Username);
                    
                    if (existingUser != null)
                    {
                        MessageBox.Show("Username already exists. Please use a unique username.", 
                            "Duplicate Username", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StatusText.Text = "Ready";
                        return;
                    }
                    
                    await _unitOfWork.Users.AddAsync(dialog.UserData);
                    await LoadUserDataAsync();
                    
                    MessageBox.Show("User added successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "User added successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding user: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error adding user";
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user to edit.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check if editing own account
            bool isEditingOwnAccount = _selectedUser.Id == _authService.CurrentUser?.Id;

            // Allow self-editing but with restrictions
            var dialog = new UserDialog(_selectedUser, isEditingOwnAccount);
            
            if (dialog.ShowDialog() == true && dialog.UserData != null)
            {
                try
                {
                    // Validate own account changes
                    if (isEditingOwnAccount)
                    {
                        // Prevent role change on own account
                        if (dialog.UserData.Role != _selectedUser.Role)
                        {
                            MessageBox.Show("You cannot change your own role. Please ask another admin.", 
                                "Cannot Change Own Role", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Prevent deactivating own account
                        if (!dialog.UserData.IsActive)
                        {
                            MessageBox.Show("You cannot deactivate your own account. Please ask another admin.", 
                                "Cannot Deactivate Own Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    StatusText.Text = "Updating user...";
                    
                    await _unitOfWork.Users.UpdateAsync(dialog.UserData);
                    await LoadUserDataAsync();
                    
                    MessageBox.Show("User updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "User updated successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating user: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error updating user";
                }
            }
        }

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user to change password.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new ChangePasswordDialog(_selectedUser);
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    StatusText.Text = "Changing password...";
                    
                    // Update password hash
                    _selectedUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dialog.NewPassword);
                    await _unitOfWork.Users.UpdateAsync(_selectedUser);
                    
                    MessageBox.Show($"Password changed successfully for user '{_selectedUser.Username}'!", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "Password changed successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error changing password: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error changing password";
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Prevent deleting own account
            if (_selectedUser.Id == _authService.CurrentUser?.Id)
            {
                MessageBox.Show("You cannot delete your own account.", "Cannot Delete", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Prevent deleting last admin
            var adminCount = _allUsers.Count(u => u.Role == "Admin" && u.IsActive);
            if (_selectedUser.Role == "Admin" && adminCount <= 1)
            {
                MessageBox.Show("Cannot delete the last active admin user. At least one admin must exist.", 
                    "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{_selectedUser.Username}'?\n\n" +
                $"Full Name: {_selectedUser.FullName}\n" +
                $"Role: {_selectedUser.Role}\n\n" +
                $"This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    StatusText.Text = "Deleting user...";
                    
                    await _unitOfWork.Users.DeleteAsync(_selectedUser);
                    await LoadUserDataAsync();
                    
                    MessageBox.Show("User deleted successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusText.Text = "User deleted successfully";
                    _selectedUser = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error deleting user";
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadUserDataAsync();
            _selectedUser = null;
            UserDataGrid.SelectedItem = null;
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
                UserDataGrid.ItemsSource = _allUsers;
                UpdateRecordCount();
                StatusText.Text = "Ready";
                return;
            }

            try
            {
                var filtered = _allUsers.Where(u =>
                    u.Username.ToLower().Contains(searchText) ||
                    u.FullName.ToLower().Contains(searchText) ||
                    u.Email.ToLower().Contains(searchText) ||
                    u.PhoneNumber.ToLower().Contains(searchText) ||
                    u.Role.ToLower().Contains(searchText)
                ).ToList();

                UserDataGrid.ItemsSource = filtered;
                UpdateRecordCount();
                StatusText.Text = $"Search: {filtered.Count} user(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
