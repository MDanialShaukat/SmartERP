using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(SmartERPDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByCustomerCodeAsync(string customerCode)
        {
            return await _dbSet
                .Include(c => c.CreatedByUser)
                .Include(c => c.LastModifiedByUser)
                .FirstOrDefaultAsync(c => c.CustomerCode == customerCode);
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Include(c => c.CreatedByUser)
                .Include(c => c.LastModifiedByUser)
                .Where(c => c.IsActive)
                .OrderBy(c => c.CustomerName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();
            
            return await _dbSet
                .Include(c => c.CreatedByUser)
                .Include(c => c.LastModifiedByUser)
                .Where(c => c.CustomerName.ToLower().Contains(searchTerm) ||
                           c.CustomerCode.ToLower().Contains(searchTerm) ||
                           c.PhoneNumber.Contains(searchTerm) ||
                           c.Email.ToLower().Contains(searchTerm))
                .OrderBy(c => c.CustomerName)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.CreatedByUser)
                .Include(c => c.LastModifiedByUser)
                .ToListAsync();
        }

        public override async Task<Customer?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.CreatedByUser)
                .Include(c => c.LastModifiedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
