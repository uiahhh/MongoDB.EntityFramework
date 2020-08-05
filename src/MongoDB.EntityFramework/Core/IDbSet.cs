using System.Threading.Tasks;

namespace MongoDB.EntityFramework.Core
{
    public interface IDbSet<TEntity> : IDocumentQueryable<TEntity>
            where TEntity : class
    {
        void Add(TEntity entity);

        void Remove(TEntity entity);

        Task<TEntity> FindAsync<TId>(TId id);
    }
}