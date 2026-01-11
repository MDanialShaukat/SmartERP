using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public class BillingRepository : Repository<Billing>, IBillingRepository
    {
        public BillingRepository(SmartERPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Billing>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingYear)
                .ThenByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task<IEnumerable<Billing>> GetPendingBillsAsync()
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Where(b => b.PaymentStatus == "Pending" || b.PaymentStatus == "Partial")
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Billing>> GetOverdueBillsAsync()
        {
            var today = DateTime.Now.Date;
            
            return await _dbSet
                .Include(b => b.Customer)
                .Where(b => (b.PaymentStatus == "Pending" || b.PaymentStatus == "Partial") 
                           && b.DueDate < today)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Billing>> GetBillsByMonthYearAsync(int month, int year)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Where(b => b.BillingMonth == month && b.BillingYear == year)
                .OrderBy(b => b.Customer!.CustomerName)
                .ToListAsync();
        }

        public async Task<Billing?> GetByBillNumberAsync(string billNumber)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BillNumber == billNumber);
        }

        public async Task<string> GenerateBillNumberAsync()
        {
            var year = DateTime.Now.Year.ToString();
            var month = DateTime.Now.Month.ToString("D2");
            
            var lastBill = await _dbSet
                .Where(b => b.BillNumber.StartsWith($"BILL-{year}{month}"))
                .OrderByDescending(b => b.BillNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            
            if (lastBill != null)
            {
                var lastNumberPart = lastBill.BillNumber.Split('-').Last();
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"BILL-{year}{month}-{nextNumber:D4}";
        }
    }
}
