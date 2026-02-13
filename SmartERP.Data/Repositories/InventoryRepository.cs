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
                .Include(i => i.CreatedByUser)
                .Include(i => i.LastModifiedByUser)
                .Where(i => i.QuantityRemaining <= threshold)
                .OrderBy(i => i.QuantityRemaining)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetByCategoryAsync(string category)
        {
            return await _dbSet
                .Include(i => i.CreatedByUser)
                .Include(i => i.LastModifiedByUser)
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

        public async Task<InventoryAssignment> AssignInventoryAsync(int inventoryId, int userId, int quantityAssigned, string remarks = "", int? createdBy = null)
        {
            var inventory = await GetByIdAsync(inventoryId);
            
            if (inventory == null)
                throw new InvalidOperationException("Inventory item not found");

            if (quantityAssigned <= 0)
                throw new InvalidOperationException("Quantity assigned must be greater than zero");

            if (inventory.QuantityRemaining < quantityAssigned)
                throw new InvalidOperationException($"Insufficient inventory. Available: {inventory.QuantityRemaining}, Requested: {quantityAssigned}");

            // Create the assignment record
            var assignment = new InventoryAssignment
            {
                InventoryId = inventoryId,
                AssignedToUserId = userId,
                QuantityAssigned = quantityAssigned,
                AssignmentDate = DateTime.Now,
                Remarks = remarks,
                CreatedDate = DateTime.Now,
                CreatedBy = createdBy
            };

            // Deduct from available inventory
            inventory.QuantityUsed += quantityAssigned;
            inventory.QuantityRemaining = inventory.QuantityPurchased - inventory.QuantityUsed;
            inventory.LastModifiedDate = DateTime.Now;

            // Add assignment to context
            _context.Set<InventoryAssignment>().Add(assignment);
            
            // Update inventory in context
            _context.Set<Inventory>().Update(inventory);
            
            // Save both changes in a single operation
            await _context.SaveChangesAsync();

            return assignment;
        }

        public override async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _dbSet
                .Include(i => i.CreatedByUser)
                .Include(i => i.LastModifiedByUser)
                .ToListAsync();
        }

        public override async Task<Inventory?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(i => i.CreatedByUser)
                .Include(i => i.LastModifiedByUser)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}
