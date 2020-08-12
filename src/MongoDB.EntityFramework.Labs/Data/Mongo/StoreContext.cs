﻿using System;
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
        public StoreContext(IMongoClient client, MongoSettings settings)
            : base(client, settings.DatabaseName)
        {
            //TODO: lazy
            Orders = new DbSet<Order, Guid>(this);
            Items = new DbSet<Item, ItemId>(this);
            Boxes = new DbSet<Box, BoxId>(this);
        }

        public DbSet<Order, Guid> Orders { get; set; }

        public DbSet<Item, ItemId> Items { get; set; }

        public DbSet<Box, BoxId> Boxes { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    builder.Entity<Order>().HasKey(m => m.Id);
        //    base.OnModelCreating(builder);
        //}
    }
}