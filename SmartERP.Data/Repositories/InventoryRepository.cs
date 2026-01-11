using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public class InventoryRepository : Repository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(SmartERPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync(int threshold = 10)
        {
            return await _dbSet
                .Where(i => i.QuantityRemaining <= threshold)
                .OrderBy(i => i.QuantityRemaining)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(i => i.Category == category)
                .OrderBy(i => i.ItemName)
                .ToListAsync();
        }

        public async Task UpdateQuantityAsync(int inventoryId, int quantityUsed)
        {
            var inventory = await GetByIdAsync(inventoryId);
            
            if (inventory == null)
                throw new InvalidOperationException("Inventory item not found");

            inventory.QuantityUsed += quantityUsed;
            inventory.QuantityRemaining = inventory.QuantityPurchased - inventory.QuantityUsed;
            inventory.LastModifiedDate = DateTime.Now;

            await UpdateAsync(inventory);
        }
    }
}
