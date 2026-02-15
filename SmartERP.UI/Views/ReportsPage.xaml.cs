using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmartERP.Core.Services;
using SmartERP.Data;

namespace SmartERP.UI.Views
{
    public partial class ReportsPage : Page
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authService;

        public ReportsPage(IUnitOfWork unitOfWork, IAuthenticationService authService)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        // Billing Reports
        private void MonthlyBillingReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.MonthlyBilling);
            dialog.ShowDialog();
        }

        private void OverdueBillsReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.OverdueBills);
            dialog.ShowDialog();
        }

        private void PaymentCollectionReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.PaymentCollection);
            dialog.ShowDialog();
        }

        // Customer Reports
        private void ActiveCustomersReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.ActiveCustomers);
            dialog.ShowDialog();
        }

        private void CustomerOutstandingReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.CustomerOutstanding);
            dialog.ShowDialog();
        }

        private void NewCustomersReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.NewCustomers);
            dialog.ShowDialog();
        }

        // Inventory Reports
        private void StockStatusReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.StockStatus);
            dialog.ShowDialog();
        }

        private void LowStockReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.LowStock);
            dialog.ShowDialog();
        }

        private void InventoryValueReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.InventoryValue);
            dialog.ShowDialog();
        }

        // Inventory Assignment Reports
        private void AssignmentSummaryReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.AssignmentSummary);
            dialog.ShowDialog();
        }

        private void AssignmentByUserReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.AssignmentByUser);
            dialog.ShowDialog();
        }

        private void AssignmentByItemReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.AssignmentByItem);
            dialog.ShowDialog();
        }

        private void AssignmentDetailsReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.AssignmentDetails);
            dialog.ShowDialog();
        }

        private void AssignmentTrendReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.AssignmentTrend);
            dialog.ShowDialog();
        }

        // System Reports
        private void UserActivityReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.UserActivity);
            dialog.ShowDialog();
        }

        private void DashboardSummaryReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportViewerDialog(_unitOfWork, _authService, ReportType.DashboardSummary);
            dialog.ShowDialog();
        }
    }

    public enum ReportType
    {
        MonthlyBilling,
        OverdueBills,
        PaymentCollection,
        ActiveCustomers,
        CustomerOutstanding,
        NewCustomers,
        StockStatus,
        LowStock,
        InventoryValue,
        AssignmentSummary,
        AssignmentByUser,
        AssignmentByItem,
        AssignmentDetails,
        AssignmentTrend,
        UserActivity,
        DashboardSummary
    }
}
