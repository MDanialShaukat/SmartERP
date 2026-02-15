using System.Collections.Generic;
using System.Threading.Tasks;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public interface IRecoveryPersonRepository : IRepository<RecoveryPerson>
    {
        Task<IEnumerable<RecoveryPerson>> GetActiveRecoveryPersonsAsync();
        Task<RecoveryPerson?> GetByNameAsync(string personName);
    }
}
