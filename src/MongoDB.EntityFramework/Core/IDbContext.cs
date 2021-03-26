using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.EntityFramework.Core
{
    public interface IDbContext
    {
        IDbSet<TEntity, TId> Set<TEntity, TId>() where TEntity : class;

        Task<List<TEntity>> ToListAsync<TEntity, TId>(
            Expression<Func<TEntity, bool>> filter, 
            PagedOptions<TEntity> pagedOptions = null,
            List<OrderDefinition<TEntity>> orderDefinitions = null,
            int? skipCount = null,
            int? limitCount = null,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default) 
            where TEntity : class;

        Task<TEntity> FirstOrDefaultAsync<TEntity, TId>(
            Expression<Func<TEntity, bool>> filter, 
            CancellationToken cancellationToken = default) 
            where TEntity : class;

        Task<TEntity> FindAsync<TEntity, TId>(TId id, CancellationToken cancellationToken = default) where TEntity : class;

        TEntity Add<TEntity, TId>(TEntity entity) where TEntity : class;

        TEntity Update<TEntity, TId>(TEntity entity) where TEntity : class;

        void Remove<TEntity, TId>(TEntity entity) where TEntity : class;

        void Remove<TEntity, TId>(object id) where TEntity : class;

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        void SetCollectionName<TEntity>(string collectionName) where TEntity : class;

        void ClearContext();

        //create a bulk
        //db.collection.bulkWrite()
    }
}