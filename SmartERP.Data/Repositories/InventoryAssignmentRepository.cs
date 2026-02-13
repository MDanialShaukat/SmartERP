using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public class InventoryAssignmentRepository : Repository<InventoryAssignment>, IInventoryAssignmentRepository
    {
        public InventoryAssignmentRepository(SmartERPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<InventoryAssignment>> GetByInventoryIdAsync(int inventoryId)
        {
            return await _dbSet
                .Include(ia => ia.Inventory)
                .Include(ia => ia.AssignedToUser)
                .Include(ia => ia.CreatedByUser)
                .Where(ia => ia.InventoryId == inventoryId)
                .OrderByDescending(ia => ia.AssignmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryAssignment>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(ia => ia.Inventory)
                .Include(ia => ia.AssignedToUser)
                .Include(ia => ia.CreatedByUser)
                .Where(ia => ia.AssignedToUserId == userId)
                .OrderByDescending(ia => ia.AssignmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryAssignment>> GetAssignmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(ia => ia.Inventory)
                .Include(ia => ia.AssignedToUser)
                .Include(ia => ia.CreatedByUser)
                .Where(ia => ia.AssignmentDate >= startDate && ia.AssignmentDate <= endDate)
                .OrderByDescending(ia => ia.AssignmentDate)
                .ToListAsync();
        }

        public override async Task<IEnumerable<InventoryAssignment>> GetAllAsync()
        {
            return await _dbSet
                .Include(ia => ia.Inventory)
                .Include(ia => ia.AssignedToUser)
                .Include(ia => ia.CreatedByUser)
                .OrderByDescending(ia => ia.AssignmentDate)
                .ToListAsync();
        }

        public override async Task<InventoryAssignment?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(ia => ia.Inventory)
                .Include(ia => ia.AssignedToUser)
                .Include(ia => ia.CreatedByUser)
                .FirstOrDefaultAsync(ia => ia.Id == id);
        }
    }
}
