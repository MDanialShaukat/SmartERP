using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartERP.Data;
using SmartERP.Models.Entities;

namespace SmartERP.Core.Services
{
    /// <summary>
    /// Service for generating inventory assignment reports
    /// </summary>
    public interface IInventoryAssignmentReportService
    {
        Task<AssignmentSummaryReport> GetAssignmentSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<UserAssignmentReport>> GetAssignmentsByUserAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ItemAssignmentReport>> GetAssignmentsByItemAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<AssignmentDetailReport>> GetDetailedAssignmentsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<AssignmentTrendReport> GetAssignmentTrendAsync(int monthsBack = 12);
    }

    public class InventoryAssignmentReportService : IInventoryAssignmentReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryAssignmentReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get overall assignment summary report
        /// </summary>
        public async Task<AssignmentSummaryReport> GetAssignmentSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddYears(-1);
            endDate ??= DateTime.Now;

            var assignments = await _unitOfWork.InventoryAssignments.GetAssignmentsByDateRangeAsync(startDate.Value, endDate.Value);
            var assignmentList = assignments.ToList();

            var summary = new AssignmentSummaryReport
            {
                ReportGeneratedDate = DateTime.Now,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalAssignments = assignmentList.Count,
                TotalQuantityAssigned = assignmentList.Sum(a => a.QuantityAssigned),
                UniqueItemsAssigned = assignmentList.Select(a => a.InventoryId).Distinct().Count(),
                UniqueUsersAssigned = assignmentList.Select(a => a.AssignedToUserId).Distinct().Count(),
                AverageQuantityPerAssignment = assignmentList.Any() ? assignmentList.Average(a => a.QuantityAssigned) : 0,
                AverageAssignmentsPerDay = assignmentList.Any() ? (double)assignmentList.Count / (endDate.Value - startDate.Value).TotalDays : 0
            };

            return summary;
        }

        /// <summary>
        /// Get assignments grouped by user
        /// </summary>
        public async Task<List<UserAssignmentReport>> GetAssignmentsByUserAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            var assignments = await _unitOfWork.InventoryAssignments.GetAssignmentsByDateRangeAsync(startDate.Value, endDate.Value);

            var userReports = assignments
                .GroupBy(a => a.AssignedToUser)
                .Select(g => new UserAssignmentReport
                {
                    UserId = g.Key?.Id ?? 0,
                    UserName = g.Key?.FullName ?? "Unknown",
                    TotalAssignments = g.Count(),
                    TotalQuantityReceived = g.Sum(a => a.QuantityAssigned),
                    UniqueItemsReceived = g.Select(a => a.InventoryId).Distinct().Count(),
                    FirstAssignmentDate = g.Min(a => a.AssignmentDate),
                    LastAssignmentDate = g.Max(a => a.AssignmentDate),
                    AverageQuantityPerAssignment = g.Average(a => a.QuantityAssigned)
                })
                .OrderByDescending(r => r.TotalQuantityReceived)
                .ToList();

            return userReports;
        }

        /// <summary>
        /// Get assignments grouped by inventory item
        /// </summary>
        public async Task<List<ItemAssignmentReport>> GetAssignmentsByItemAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            var assignments = await _unitOfWork.InventoryAssignments.GetAssignmentsByDateRangeAsync(startDate.Value, endDate.Value);

            var itemReports = assignments
                .GroupBy(a => a.Inventory)
                .Select(g => new ItemAssignmentReport
                {
                    InventoryId = g.Key?.Id ?? 0,
                    ItemName = g.Key?.ItemName ?? "Unknown",
                    Category = g.Key?.Category ?? "Unknown",
                    TotalAssignments = g.Count(),
                    TotalQuantityAssigned = g.Sum(a => a.QuantityAssigned),
                    UniqueUsersReceived = g.Select(a => a.AssignedToUserId).Distinct().Count(),
                    FirstAssignmentDate = g.Min(a => a.AssignmentDate),
                    LastAssignmentDate = g.Max(a => a.AssignmentDate),
                    AverageQuantityPerAssignment = g.Average(a => a.QuantityAssigned),
                    CurrentStock = g.Key?.QuantityRemaining ?? 0,
                    TotalPurchased = g.Key?.QuantityPurchased ?? 0
                })
                .OrderByDescending(r => r.TotalQuantityAssigned)
                .ToList();

            return itemReports;
        }

        /// <summary>
        /// Get detailed assignment records
        /// </summary>
        public async Task<List<AssignmentDetailReport>> GetDetailedAssignmentsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-3);
            endDate ??= DateTime.Now;

            var assignments = await _unitOfWork.InventoryAssignments.GetAssignmentsByDateRangeAsync(startDate.Value, endDate.Value);

            var detailReports = assignments
                .Select(a => new AssignmentDetailReport
                {
                    AssignmentId = a.Id,
                    ItemName = a.Inventory?.ItemName ?? "Unknown",
                    Category = a.Inventory?.Category ?? "Unknown",
                    QuantityAssigned = a.QuantityAssigned,
                    AssignedToUser = a.AssignedToUser?.FullName ?? "Unknown",
                    AssignmentDate = a.AssignmentDate,
                    Remarks = a.Remarks,
                    AssignedByUser = a.CreatedByUser?.FullName ?? "System",
                    CreatedDate = a.CreatedDate
                })
                .OrderByDescending(r => r.AssignmentDate)
                .ToList();

            return detailReports;
        }

        /// <summary>
        /// Get assignment trend over time
        /// </summary>
        public async Task<AssignmentTrendReport> GetAssignmentTrendAsync(int monthsBack = 12)
        {
            var startDate = DateTime.Now.AddMonths(-monthsBack);
            var endDate = DateTime.Now;

            var assignments = await _unitOfWork.InventoryAssignments.GetAssignmentsByDateRangeAsync(startDate, endDate);
            var assignmentList = assignments.ToList();

            var monthlyTrend = new List<MonthlyTrendData>();

            for (int i = monthsBack - 1; i >= 0; i--)
            {
                var monthStart = DateTime.Now.AddMonths(-i).Date;
                monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthAssignments = assignmentList
                    .Where(a => a.AssignmentDate >= monthStart && a.AssignmentDate <= monthEnd)
                    .ToList();

                monthlyTrend.Add(new MonthlyTrendData
                {
                    Month = monthStart.ToString("MMMM yyyy"),
                    AssignmentCount = monthAssignments.Count,
                    TotalQuantity = monthAssignments.Sum(a => a.QuantityAssigned),
                    UniqueItems = monthAssignments.Select(a => a.InventoryId).Distinct().Count(),
                    UniqueUsers = monthAssignments.Select(a => a.AssignedToUserId).Distinct().Count()
                });
            }

            return new AssignmentTrendReport
            {
                ReportGeneratedDate = DateTime.Now,
                MonthsAnalyzed = monthsBack,
                MonthlyData = monthlyTrend,
                AverageAssignmentsPerMonth = monthlyTrend.Average(m => m.AssignmentCount),
                AverageQuantityPerMonth = monthlyTrend.Average(m => m.TotalQuantity),
                HighestAssignmentMonth = monthlyTrend.OrderByDescending(m => m.AssignmentCount).FirstOrDefault()?.Month ?? "N/A"
            };
        }
    }

    // Report DTOs
    public class AssignmentSummaryReport
    {
        public DateTime ReportGeneratedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalQuantityAssigned { get; set; }
        public int UniqueItemsAssigned { get; set; }
        public int UniqueUsersAssigned { get; set; }
        public double AverageQuantityPerAssignment { get; set; }
        public double AverageAssignmentsPerDay { get; set; }
    }

    public class UserAssignmentReport
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int TotalQuantityReceived { get; set; }
        public int UniqueItemsReceived { get; set; }
        public DateTime FirstAssignmentDate { get; set; }
        public DateTime LastAssignmentDate { get; set; }
        public double AverageQuantityPerAssignment { get; set; }
    }

    public class ItemAssignmentReport
    {
        public int InventoryId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int TotalQuantityAssigned { get; set; }
        public int UniqueUsersReceived { get; set; }
        public DateTime FirstAssignmentDate { get; set; }
        public DateTime LastAssignmentDate { get; set; }
        public double AverageQuantityPerAssignment { get; set; }
        public int CurrentStock { get; set; }
        public int TotalPurchased { get; set; }
    }

    public class AssignmentDetailReport
    {
        public int AssignmentId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantityAssigned { get; set; }
        public string AssignedToUser { get; set; } = string.Empty;
        public DateTime AssignmentDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string AssignedByUser { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class AssignmentTrendReport
    {
        public DateTime ReportGeneratedDate { get; set; }
        public int MonthsAnalyzed { get; set; }
        public List<MonthlyTrendData> MonthlyData { get; set; } = new();
        public double AverageAssignmentsPerMonth { get; set; }
        public double AverageQuantityPerMonth { get; set; }
        public string HighestAssignmentMonth { get; set; } = string.Empty;
    }

    public class MonthlyTrendData
    {
        public string Month { get; set; } = string.Empty;
        public int AssignmentCount { get; set; }
        public int TotalQuantity { get; set; }
        public int UniqueItems { get; set; }
        public int UniqueUsers { get; set; }
    }
}
