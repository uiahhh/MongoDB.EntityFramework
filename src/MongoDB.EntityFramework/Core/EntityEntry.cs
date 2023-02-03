namespace MongoDB.EntityFramework.Core
{
    public class EntityEntry<TEntity> 
        where TEntity : class
    {
        public EntityEntry(TEntity entity)
        {
            Entity = entity;
        }

        //TODO: include this in the constructor
        //public EntityState EntityState { get; }

        public TEntity Entity { get; }
    }
}
