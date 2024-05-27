using API.Core.Contracts;
using API.Data;
using API.Entities;

namespace API.Core.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConnectifyDbContext _context;

        public IGenericRepository<AppUser> User { get; private set; }

        public UnitOfWork(ConnectifyDbContext context)
        {
            _context = context;

            User = new GenericRepository<AppUser>(_context);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }

        public Task SaveAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
