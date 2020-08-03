using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFramework.Samples.Entities;

namespace MongoDB.EntityFramework.Samples.Data.Sqlite
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Order>().HasKey(m => m.Id);
            base.OnModelCreating(builder);
        }
    }
}