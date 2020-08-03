using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.EntityFramework.Core
{
    public interface IDbContext
    {
        IDbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<List<TEntity>> AllAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class;

        Task<List<TEntity>> ToListAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class;

        Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class;

        Task<TEntity> FindAsync<TEntity>(object id, CancellationToken cancellationToken = default) where TEntity : class;

        TEntity Add<TEntity>(TEntity entity) where TEntity : class;

        TEntity Update<TEntity>(TEntity entity) where TEntity : class;

        void Remove<TEntity>(TEntity entity) where TEntity : class;

        void Remove<TEntity>(object id) where TEntity : class;

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        //create a bulk
        //db.collection.bulkWrite()
    }
}