using API.Entities;

namespace API.Core.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<AppUser> User { get; }

        Task SaveAsync();
        bool HasChanges();
    }
}
