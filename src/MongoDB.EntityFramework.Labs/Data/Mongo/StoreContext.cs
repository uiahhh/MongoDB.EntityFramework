using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.EntityFramework.Core;
using MongoDB.EntityFramework.Samples.Entities;

namespace MongoDB.EntityFramework.Samples.Data.Mongo
{
    public class MongoSettings
    {
        public MongoSettings(string connectionString, string databaseName)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
        }

        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }

    public class StoreContext : DbContext
    {
        private static bool indexesCreated;

        public StoreContext(IMongoClient client, MongoSettings settings)
            : base(client, settings.DatabaseName)
        {
            EntityObjectIds = new DbSet<EntityObjectId, ObjectId>(this);
            EntityGuids = new DbSet<EntityGuid, Guid>(this);

            //TODO: lazy
            Orders = new DbSet<Order, Guid>(this);
            //OrdersBkp = new DbSet<Order, Guid>(this, nameof(OrdersBkp));
            OrdersFlat = new DbSet<OrderFlat, Guid>(this, nameof(Order));
            Items = new DbSet<Item, ItemId>(this);
            Boxes = new DbSet<Box, BoxId>(this);

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            if (indexesCreated)
            {
                CreateDescendingIndex<EntityObjectId>(x => x.CreatedAt);
                CreateDescendingIndex<EntityGuid>(x => x.CreatedAt);

                //CreateAscendingIndex<EntityObjectId>(x => x.ProjectId);
                //CreateAscendingIndex<EntityGuid>(x => x.ProjectId);
                CreateProjectIndex<EntityObjectId, ObjectId>();
                CreateProjectIndex<EntityGuid, Guid>();

                var collection = this.GetCollection<EntityObjectId>();
                var indexKeysDefinition = Builders<EntityObjectId>.IndexKeys.Descending(x => x.CreatedAt);
                collection.Indexes.CreateOne(new CreateIndexModel<EntityObjectId>(indexKeysDefinition));

                var collection2 = this.GetCollection<EntityGuid>();
                var indexKeysDefinition2 = Builders<EntityGuid>.IndexKeys.Descending(x => x.CreatedAt);
                collection2.Indexes.CreateOne(new CreateIndexModel<EntityGuid>(indexKeysDefinition2));

                indexesCreated = true;
            }
        }

        private void CreateProjectIndex<TEntity, TId>()
            where TEntity : class, IEntity<TId>
            where TId : IEquatable<TId>
        {
            var builder = Builders<TEntity>.IndexKeys;
            var index = builder.Hashed(x => x.ProjectId);
            CreateIndex(index);
        }

        private void CreateDescendingIndex<TEntity>(Expression<Func<TEntity, object>> field)
            where TEntity : class
        {
            var collection = this.GetCollection<TEntity>();
            var indexKeysDefinition = Builders<TEntity>.IndexKeys.Descending(field);
            collection.Indexes.CreateOne(new CreateIndexModel<TEntity>(indexKeysDefinition));
        }

        private void CreateAscendingIndex<TEntity>(Expression<Func<TEntity, object>> field)
            where TEntity : class
        {
            var collection = this.GetCollection<TEntity>();
            var indexKeysDefinition = Builders<TEntity>.IndexKeys.Ascending(field);
            collection.Indexes.CreateOne(new CreateIndexModel<TEntity>(indexKeysDefinition));
        }

        private void CreateIndex<TEntity>(IndexKeysDefinition<TEntity> index)
            where TEntity : class
        {
            var collection = this.GetCollection<TEntity>();
            var indexModel = new CreateIndexModel<TEntity>(index);
            collection.Indexes.CreateOne(indexModel);
        }

        public DbSet<EntityObjectId, ObjectId> EntityObjectIds { get; set; }
        public DbSet<EntityGuid, Guid> EntityGuids { get; set; }

        public DbSet<Order, Guid> Orders { get; set; }

        //public DbSet<Order, Guid> OrdersBkp { get; set; }

        public DbSet<OrderFlat, Guid> OrdersFlat { get; set; }

        public DbSet<Item, ItemId> Items { get; set; }

        public DbSet<Box, BoxId> Boxes { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    builder.Entity<Order>().HasKey(m => m.Id);
        //    base.OnModelCreating(builder);
        //}
    }
}