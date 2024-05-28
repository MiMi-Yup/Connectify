using API.Entities;

namespace API.Core.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<AppUser> Users { get; }
        IGenericRepository<Contact> Contacts { get; }
        IGenericRepository<Group> Groups { get; }

        Task SaveAsync();
        bool HasChanges();
    }
}
