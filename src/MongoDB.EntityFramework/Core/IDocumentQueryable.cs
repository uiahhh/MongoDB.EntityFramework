using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDB.EntityFramework.Core
{
    public interface IDocumentQueryable<TEntity>
    {
        Task<List<TEntity>> ToListAsync();

        Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> FirstOrDefaultAsync();

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        IDocumentQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    }
}