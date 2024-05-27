using System.Linq.Expressions;

namespace API.Core.Contracts
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAll(string includeProperties = "", bool trackChanges = true);
        Task<TEntity?> GetByID<TId>(TId id, bool trackChanges = true);
        IQueryable<TEntity> Find(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool trackChanges = true);
        Task<bool> Insert(TEntity entity);
        void Update(TEntity entity);
        Task<int> Update(Expression<Func<Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<TEntity>,
                Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<TEntity>>> expression);
        Task<bool> Delete<TId>(TId id);
        void Delete(TEntity entityToDelete);
    }
}
