using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public interface IBillingRepository : IRepository<Billing>
    {
        Task<IEnumerable<Billing>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Billing>> GetPendingBillsAsync();
        Task<IEnumerable<Billing>> GetOverdueBillsAsync();
        Task<IEnumerable<Billing>> GetBillsByMonthYearAsync(int month, int year);
        Task<Billing?> GetByBillNumberAsync(string billNumber);
        Task<string> GenerateBillNumberAsync();
    }
}
