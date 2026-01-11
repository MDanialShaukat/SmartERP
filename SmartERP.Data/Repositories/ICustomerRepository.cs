using System.Collections.Generic;
using System.Threading.Tasks;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByCustomerCodeAsync(string customerCode);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
    }
}
