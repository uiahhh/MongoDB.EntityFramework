using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.EntityFramework.Expressions;

namespace MongoDB.EntityFramework.Core
{
    //TODO: async methods
    //TODO: colocar cancellationtoken nos methodos async

    public class DbSet<TEntity> : IDbSet<TEntity>
           where TEntity : class
    {
        private readonly IDbContext context;

        private Expression<Func<TEntity, bool>> predicate = noFilter;
        private static Expression<Func<TEntity, bool>> noFilter = _ => true;

        public DbSet(IDbContext context)
        {
            this.context = context;
        }

        public async Task<TEntity> FindAsync<TId>(TId id)
        {
            return await this.context.FindAsync<TEntity, TId>(id);
        }

        public async Task<List<TEntity>> ToListAsync()
        {
            var result = await this.context.ToListAsync(this.predicate);

            this.predicate = noFilter;

            return result;
        }

        public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await this.Where(predicate).ToListAsync();
        }

        public async Task<TEntity> FirstOrDefaultAsync()
        {
            //TODO: executar teste em paralelo e ver se o predicate influencia em outra query
            var result = await this.context.FirstOrDefaultAsync(this.predicate);

            this.predicate = noFilter;

            return result;
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await this.Where(predicate).FirstOrDefaultAsync();
        }

        public IDocumentQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            if (this.predicate == noFilter)
            {
                this.predicate = predicate;
            }
            else
            {
                this.predicate = this.predicate.AndAlso(predicate);
            }

            return this;
        }

        public void Add(TEntity entity)
        {
            this.context.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            this.context.Remove(entity);
        }
    }
}