using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDB.EntityFramework.Core
{
    public class ChangeTracker
    {
        public ChangeTracker(DbContext dbContext)
        {
            DbContext = dbContext;
        }

        public DbContext DbContext { get; }

        public IEnumerable<EntityEntry<TEntity>> Entries<TEntity>() 
            where TEntity : class
        {
            return DbContext.Entries<TEntity>();
        }
    }
}
