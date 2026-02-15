using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public class AreaRepository : Repository<Area>, IAreaRepository
    {
        public AreaRepository(SmartERPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Area>> GetActiveAreasAsync()
        {
            return await _dbSet
                .Where(a => a.IsActive)
                .Include(a => a.CreatedByUser)
                .Include(a => a.LastModifiedByUser)
                .OrderBy(a => a.AreaName)
                .ToListAsync();
        }

        public async Task<Area?> GetByAreaNameAsync(string areaName)
        {
            return await _dbSet
                .Include(a => a.CreatedByUser)
                .Include(a => a.LastModifiedByUser)
                .FirstOrDefaultAsync(a => a.AreaName == areaName);
        }

        public override async Task<IEnumerable<Area>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.CreatedByUser)
                .Include(a => a.LastModifiedByUser)
                .OrderBy(a => a.AreaName)
                .ToListAsync();
        }

        public override async Task<Area?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(a => a.CreatedByUser)
                .Include(a => a.LastModifiedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
