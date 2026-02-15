using System;
using System.Threading.Tasks;
using SmartERP.Data.Repositories;

namespace SmartERP.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IAreaRepository Areas { get; }
        IInventoryRepository Inventories { get; }
        ICustomerRepository Customers { get; }
        IBillingRepository Billings { get; }
        IInventoryAssignmentRepository InventoryAssignments { get; }
        IRecoveryPersonRepository RecoveryPersons { get; }
        
        Task<int> SaveChangesAsync();
        int SaveChanges();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
