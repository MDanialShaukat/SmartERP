using System.Collections.Generic;
using System.Threading.Tasks;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public interface IAreaRepository : IRepository<Area>
    {
        Task<IEnumerable<Area>> GetActiveAreasAsync();
        Task<Area?> GetByAreaNameAsync(string areaName);
    }
}
