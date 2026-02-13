using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SmartERP.Data;
using SmartERP.Models.Entities;

namespace SmartERP.UI.Views
{
    public partial class InventoryAssignmentHistoryDialog : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        private Inventory? _inventory;

        public InventoryAssignmentHistoryDialog(IUnitOfWork unitOfWork, Inventory inventory)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _inventory = inventory;

            Loaded += AssignmentHistoryDialog_Loaded;
        }

        private async void AssignmentHistoryDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_inventory != null)
                {
                    DialogTitle.Text = $"Assignment History - {_inventory.ItemName}";

                    // Load assignment history
                    var assignments = await _unitOfWork.InventoryAssignments.GetByInventoryIdAsync(_inventory.Id);
                    var assignmentList = assignments.ToList();

                    AssignmentDataGrid.ItemsSource = assignmentList;
                    TotalAssignmentsTextBlock.Text = $"Total Assignments: {assignmentList.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assignment history: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
