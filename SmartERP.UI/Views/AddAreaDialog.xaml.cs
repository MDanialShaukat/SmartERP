using System;
using System.Windows;
using SmartERP.Data;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class AddAreaDialog : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _currentUserId;

        public Area? CreatedArea { get; private set; }

        public AddAreaDialog(IUnitOfWork unitOfWork, int currentUserId)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _currentUserId = currentUserId;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(AreaNameTextBox.Text))
            {
                MessageBox.Show("Please enter an area name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Check if area already exists
                var existingArea = await _unitOfWork.Areas.GetByAreaNameAsync(AreaNameTextBox.Text.Trim());
                if (existingArea != null)
                {
                    MessageBox.Show("An area with this name already exists.", "Duplicate Area", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new area
                var newArea = new Area
                {
                    AreaName = AreaNameTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = _currentUserId
                };

                await _unitOfWork.Areas.AddAsync(newArea);
                await _unitOfWork.SaveChangesAsync();

                CreatedArea = newArea;

                MessageBox.Show("Area added successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding area: {ex.Message}", "Error", 
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
