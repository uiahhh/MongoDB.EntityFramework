using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.EntityFramework.Expressions;

namespace MongoDB.EntityFramework.Core
{
    //TODO: async methods
    //TODO: colocar cancellationtoken nos methodos async

    public class DbSet<TEntity, TId> : IDbSet<TEntity, TId>
           where TEntity : class
    {
        private readonly IDbContext context;

        private Expression<Func<TEntity, bool>> predicate = noFilter;
        private static Expression<Func<TEntity, bool>> noFilter = _ => true;

        private List<OrderDefinition<TEntity>> orderDefinitions = new List<OrderDefinition<TEntity>>();
        private PagedOptions<TEntity> pagedOptions = null;
        private int? skipCount = null;
        private int? limitCount = null;
        private bool asNoTracking = false;

        public DbSet(IDbContext context)
            : this(context, typeof(TEntity).Name)
        {            
        }

        public DbSet(IDbContext context, string collectionName)
        {
            this.context = context;
            this.context.SetCollectionName<TEntity>(collectionName);
        }

        public async Task<TEntity> FindAsync(TId id)
        {
            return await this.context.FindAsync<TEntity, TId>(id);
        }

        public async Task<List<TEntity>> ToListAsync()
        {
            var result = await this.context.ToListAsync<TEntity, TId>(
                this.predicate, 
                this.pagedOptions,
                this.orderDefinitions,
                this.skipCount,
                this.limitCount,
                this.asNoTracking);

            ClearState();

            return result;
        }

        private void ClearState()
        {
            this.orderDefinitions.Clear();
            this.predicate = noFilter;
            this.pagedOptions = null;
            this.skipCount = null;
            this.limitCount = null;
            this.asNoTracking = false;
        }

        public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await this.Where(predicate).ToListAsync();
        }

        public async Task<List<TEntity>> ToListAsync(PagedOptions<TEntity> pagedOptions)
        {
            this.pagedOptions = pagedOptions;

            return await this.ToListAsync();
        }

        public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate, PagedOptions<TEntity> pagedOptions)
        {            
            return await this.Where(predicate).ToListAsync(pagedOptions);
        }

        public async Task<TEntity> FirstOrDefaultAsync()
        {
            //TODO: executar teste em paralelo e ver se o predicate influencia em outra query
            var result = await this.context.FirstOrDefaultAsync<TEntity, TId>(this.predicate);

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

        public IDocumentQueryable<TEntity> Skip(int count)
        {
            this.skipCount = count;

            return this;
        }

        public IDocumentQueryable<TEntity> Take(int count)
        {
            return this.Limit(count);
        }

        public IDocumentQueryable<TEntity> Limit(int count)
        {
            this.limitCount = count;

            return this;
        }

        public IOrderedQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> fieldSelector)
        {
            this.orderDefinitions.Add(new OrderDefinition<TEntity>(isDescending: false, fieldSelector));

            return this;
        }

        public IOrderedQueryable<TEntity> OrderByDescending(Expression<Func<TEntity, object>> fieldSelector)
        {
            this.orderDefinitions.Add(new OrderDefinition<TEntity>(isDescending: true, fieldSelector));

            return this;
        }

        public IOrderedQueryable<TEntity> ThenBy(Expression<Func<TEntity, object>> fieldSelector)
        {
            this.orderDefinitions.Add(new OrderDefinition<TEntity>(isDescending: false, fieldSelector));

            return this;
        }

        public IOrderedQueryable<TEntity> ThenByDescending(Expression<Func<TEntity, object>> fieldSelector)
        {
            this.orderDefinitions.Add(new OrderDefinition<TEntity>(isDescending: true, fieldSelector));

            return this;
        }

        public IDocumentQueryable<TEntity> AsNoTracking()
        {
            this.asNoTracking = true;

            return this;
        }

        public void Add(TEntity entity)
        {
            this.context.Add<TEntity, TId>(entity);
        }

        public void Remove(TEntity entity)
        {
            this.context.Remove<TEntity, TId>(entity);
        }
    }

    public class OrderDefinition<TEntity>
    {
        public OrderDefinition(bool isDescending, Expression<Func<TEntity, object>> fieldSelector)
        {
            IsDescending = isDescending;
            FieldSelector = fieldSelector;
        }

        public bool IsDescending { get; set; }

        public Expression<Func<TEntity, object>> FieldSelector { get; set; }
    }
}