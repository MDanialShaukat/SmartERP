using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public class RecoveryPersonRepository : Repository<RecoveryPerson>, IRecoveryPersonRepository
    {
        public RecoveryPersonRepository(SmartERPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RecoveryPerson>> GetActiveRecoveryPersonsAsync()
        {
            return await _dbSet
                .Where(rp => rp.IsActive)
                .OrderBy(rp => rp.PersonName)
                .ToListAsync();
        }

        public async Task<RecoveryPerson?> GetByNameAsync(string personName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rp => rp.PersonName == personName);
        }

        public override async Task<IEnumerable<RecoveryPerson>> GetAllAsync()
        {
            return await _dbSet
                .OrderBy(rp => rp.PersonName)
                .ToListAsync();
        }

        public override async Task<RecoveryPerson?> GetByIdAsync(int id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rp => rp.Id == id);
        }
    }
}
