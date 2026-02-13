using System.Collections.Generic;
using System.Threading.Tasks;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync(int threshold = 10);
        Task<IEnumerable<Inventory>> GetByCategoryAsync(string category);
        Task UpdateQuantityAsync(int inventoryId, int quantityUsed);
        Task<InventoryAssignment> AssignInventoryAsync(int inventoryId, int userId, int quantityAssigned, string remarks = "", int? createdBy = null);
    }
}
