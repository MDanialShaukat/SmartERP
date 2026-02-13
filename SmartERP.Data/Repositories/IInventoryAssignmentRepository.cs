using System.Collections.Generic;
using System.Threading.Tasks;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public interface IInventoryAssignmentRepository : IRepository<InventoryAssignment>
    {
        Task<IEnumerable<InventoryAssignment>> GetByInventoryIdAsync(int inventoryId);
        Task<IEnumerable<InventoryAssignment>> GetByUserIdAsync(int userId);
        Task<IEnumerable<InventoryAssignment>> GetAssignmentsByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
    }
}
