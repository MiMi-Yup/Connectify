using API.Core.Contracts;
using API.Data;
using API.Entities;

namespace API.Core.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConnectifyDbContext _context;

        public IGenericRepository<AppUser> Users { get; private set; }
        public IGenericRepository<Contact> Contacts { get; private set; }
        public IGenericRepository<Group> Groups { get; private set; }
        public IGenericRepository<GroupMember> GroupMembers { get; private set; }
        public IGenericRepository<Message> Messages { get; private set; }

        public UnitOfWork(ConnectifyDbContext context)
        {
            _context = context;

            Users = new GenericRepository<AppUser>(_context);
            Contacts = new GenericRepository<Contact>(_context);
            Groups = new GenericRepository<Group>(_context);
            GroupMembers = new GenericRepository<GroupMember>(_context);
            Messages = new GenericRepository<Message>(_context);
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
