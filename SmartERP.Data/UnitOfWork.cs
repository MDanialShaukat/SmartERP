using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using SmartERP.Data.Repositories;

namespace SmartERP.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SmartERPDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(SmartERPDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Areas = new AreaRepository(_context);
            Inventories = new InventoryRepository(_context);
            Customers = new CustomerRepository(_context);
            Billings = new BillingRepository(_context);
            InventoryAssignments = new InventoryAssignmentRepository(_context);
        }

        public IUserRepository Users { get; }
        public IAreaRepository Areas { get; }
        public IInventoryRepository Inventories { get; }
        public ICustomerRepository Customers { get; }
        public IBillingRepository Billings { get; }
        public IInventoryAssignmentRepository InventoryAssignments { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
