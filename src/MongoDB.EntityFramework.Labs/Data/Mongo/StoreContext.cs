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
        public StoreContext(MongoSettings settings) : base(settings.ConnectionString, settings.DatabaseName)
        {
            //TODO: lazy
            Orders = new DbSet<Order>(this);
        }

        public DbSet<Order> Orders { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    builder.Entity<Order>().HasKey(m => m.Id);
        //    base.OnModelCreating(builder);
        //}
    }
}