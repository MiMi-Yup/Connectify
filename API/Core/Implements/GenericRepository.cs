using API.Core.Contracts;
using API.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API.Core.Implements
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected ConnectifyDbContext _context;
        protected DbSet<TEntity> dbSet;

        public GenericRepository(ConnectifyDbContext context)
        {
            _context = context;
            dbSet = context.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll(string includeProperties = "", bool trackChanges = true)
        {
            IQueryable<TEntity> query = trackChanges ? dbSet : dbSet.AsNoTracking();

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<TEntity?> GetByID<TId>(TId id, bool trackChanges = true)
        {
            var item = await dbSet.FindAsync(id);

            if (item != null && !trackChanges)
            {
                _context.Entry(item).State = EntityState.Detached;
            }
            return item;
        }

        public virtual IQueryable<TEntity> Find(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool trackChanges = true)
        {
            IQueryable<TEntity> query = trackChanges ? dbSet : dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }

            return query;
        }

        public virtual async Task<bool> Insert(TEntity entity)
        {
            await dbSet.AddAsync(entity);
            return true;
        }

        public virtual void Update(TEntity entity)
        {
            dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual Task<int> Update(Expression<Func<Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<TEntity>,
            Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<TEntity>>> expression)
        {
            return dbSet.ExecuteUpdateAsync(expression);
        }

        public virtual async Task<bool> Delete<TId>(TId id)
        {
            TEntity? entityToDelete = await dbSet.FindAsync(id);
            if (entityToDelete != null)
            {
                Delete(entityToDelete);
                return true;
            }
            return false;
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }
    }
}
