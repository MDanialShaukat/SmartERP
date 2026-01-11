using System;
using System.Threading.Tasks;
using SmartERP.Data.Repositories;

namespace SmartERP.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IInventoryRepository Inventories { get; }
        ICustomerRepository Customers { get; }
        IBillingRepository Billings { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
