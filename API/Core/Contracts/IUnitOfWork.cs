using API.Entities;

namespace API.Core.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<AppUser> Users { get; }
        IGenericRepository<Contact> Contacts { get; }
        IGenericRepository<Group> Groups { get; }
        IGenericRepository<GroupMember> GroupMembers { get; }
        IGenericRepository<Message> Messages { get; }

        Task SaveAsync();
        bool HasChanges();
    }
}
