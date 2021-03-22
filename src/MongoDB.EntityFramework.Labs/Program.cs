using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using MongoDB.Driver;
using MongoDB.EntityFramework.Samples.Entities;
using Mongo = MongoDB.EntityFramework.Samples.Data.Mongo;
using Sqlite = MongoDB.EntityFramework.Samples.Data.Sqlite;

namespace MongoDB.EntityFramework.Labs
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await Task.Delay(1);

            Console.WriteLine("Sample Started");

            var serviceCollection = new ServiceCollection();

            //var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder
            //        .AddFilter("Microsoft", LogLevel.Warning)
            //        .AddFilter("System", LogLevel.Warning)
            //        .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug);
            //});
            //ILogger logger = loggerFactory.CreateLogger<Program>();
            //logger.LogInformation("Example log message");

            //TODO: user extension method
            SetupSqlite(serviceCollection);
            SetupMongo(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            await FirstTest(serviceProvider);

            //await SecondTest(serviceProvider);
        }

        private static async Task FirstTest(ServiceProvider serviceProvider)
        {
            //var sqliteContext = serviceProvider.GetService<Sqlite.StoreContext>();
            //sqliteContext.Database.EnsureCreated();
            //var id1 = Guid.NewGuid();
            //var o11 = new Order(id1, "ZehDog1", 150);
            //sqliteContext.Orders.Add(o11);
            //var o111 = sqliteContext.Orders.Find(o11.Id);
            //sqliteContext.SaveChanges();
            //var exp = sqliteContext.Orders.Where(x => x.Id == o11.Id).AsQueryable().ElementType;
            //var o1 = await sqliteContext.Orders.FirstOrDefaultAsync();

            //if (o1 != null)
            //{
            //    sqliteContext.Orders.Remove(o1);
            //    var o1111 = sqliteContext.Orders.Find(o11.Id);
            //    sqliteContext.SaveChanges();
            //}

            //var mongoClient = serviceProvider.GetService<IMongoClient>();
            //var idM = new BoxId("123");
            //var boxM = new Box(idM, 10);
            //var filterM = Builders<object>.Filter.Eq("_id", idM);
            //Expression<Func<Box, bool>> filterM2 = x => x.Id == idM;

            //var collection = mongoClient.GetDatabase("store104").GetCollection<BsonDocument>(typeof(Box).Name);
            //var collection2 = mongoClient.GetDatabase("store104").GetCollection<Box>(typeof(Box).Name);

            //var result1 = await collection2.FindAsync(filterM2);
            //var entity1 = result1.FirstOrDefault();
            //entity1.Measures = 99;

            //var id1 = typeof(Box).GetProperty("Id").GetValue(entity1, null);
            //var filterM1 = Builders<BsonDocument>.Filter.Eq("_id", (BoxId)id1);

            //var result = await collection.ReplaceOneAsync(
            //                    filterM1,
            //                    entity1.ToBsonDocument(),
            //                    new ReplaceOptions { IsUpsert = false });

            var mongoContext = serviceProvider.GetService<Mongo.StoreContext>();

            var id1 = new BoxId("555");
            var box1 = new Box(id1, 10);
            box1.Numbers = new List<int>() { 1, 2, 3 };
            box1.Configurations = new List<Configuration> { new Configuration { TenantId = new TenantId { Value = 10000 } } };
            mongoContext.Boxes.Add(box1);

            var id = new BoxId("123");
            Expression<Func<Box, bool>> filter = x => x.Configurations.Any(n => n.TenantId == new TenantId { Value = 10000 });
            //Expression<Func<Box, bool>> filter = x => x.Id == id;
            var box = await mongoContext.Boxes.FirstOrDefaultAsync(filter);
            //var box = await mongoContext.Boxes.FindAsync(id);
            //box.Measures = 234;
            //box.Numbers = new List<int>() { 1, 2, 3 };

            await mongoContext.SaveChangesAsync();

            var x = 1;

            //// var all = await mongoContext.Orders.ToListAsync();

            //var id2 = Guid.Parse("bdcbfe0d-b74b-473c-a069-c06f2c444960"); //Guid.NewGuid();
            ////var o22 = new Order(id2, "ZehDog2", 129);
            ////mongoContext.Orders.Add(o22);
            //var o222 = await mongoContext.Orders.FindAsync(id2);
            ////var o22233 = await mongoContext.Orders.FindAsync(Guid.NewGuid());

            //o222.StoreName = "sdfg";

            //await mongoContext.SaveChangesAsync();

            //var allox2 = await mongoContext.Orders.ToListAsync();

            //var id21 = 3;
            //var o221 = new Item(id21, "ZehDog2", 129);
            //mongoContext.Items.Add(o221);

            //await mongoContext.SaveChangesAsync();

            //var allo2223 = await mongoContext.Items.ToListAsync();

            //var idd = new ItemId(3, "ZehDog2");
            //var o2223 = await mongoContext.Items.FindAsync(idd);

            //var ox2 = await mongoContext.Orders.FirstOrDefaultAsync(x => x.Id == o22.Id);

            //var o2 = await mongoContext.Orders.FirstOrDefaultAsync();
            //if (o2 != null)
            //{
            //    mongoContext.Orders.Remove(o2);
            //    var o2222 = await mongoContext.Orders.FindAsync(o22.Id);
            //    await mongoContext.SaveChangesAsync();
            //}
        }

        private static async Task SecondTest(ServiceProvider serviceProvider)
        {
            var sqliteContext = serviceProvider.GetService<Sqlite.StoreContext>();
            sqliteContext.Database.EnsureCreated();

            //var id1 = Guid.Parse("a6ee1bad-3e33-4778-9f0d-fd251e5b14dc");
            var id1 = Guid.Parse("ccf25f53-3e59-4a0b-8ea9-35f94b41ea66");

            var o1NOVO = new Order(id1, "ZehDog NOVO ADDED", 150);
            sqliteContext.Orders.Add(o1NOVO);

            var o1FromContext = sqliteContext.Orders.Find(id1);
            o1FromContext.TotalValue = 111;

            var o1FromDBById = await sqliteContext.Orders.Where(x => x.Id == id1).ToListAsync();
            var o1FromDB = await sqliteContext.Orders.Where(x => x.StoreName.Equals("ZehDog NOVO")).FirstOrDefaultAsync();
            //var o1FromDB = await sqliteContext.Orders.FirstOrDefaultAsync(x => x.Id == id1);
            //o1FromDB.TotalValue = 222;

            //var id1 = Guid.NewGuid();
            //var o1 = new Order(id1, "ZehDog1", 150);
            //sqliteContext.Orders.Add(o1);
            //sqliteContext.SaveChanges();

            //var o1Clone = new Order(id1, "ZehDog1Clone", 150);
            //sqliteContext.Orders.Add(o1);
            //sqliteContext.Orders.Add(o1);
            //sqliteContext.Orders.Remove(o1FromContext);
            //o1FromContext.TotalValue = 250;
            //sqliteContext.Orders.Update(o1FromContext);

            //sqliteContext.Orders.Update(o1Clone);
            //var o1FromContext = sqliteContext.Orders.Find(id1);
            sqliteContext.SaveChanges();

            //o1FromContext.TotalValue = 380;
            //sqliteContext.Orders.Update(o1FromContext); //não salva e não causa erro
            //sqliteContext.SaveChanges();

            //var o111 = sqliteContext.Orders.Find(o11.Id);
            //var exp = sqliteContext.Orders.Where(x => x.Id == o11.Id).AsQueryable().ElementType;
            //var o1 = await sqliteContext.Orders.FirstOrDefaultAsync();

            //if (o1 != null)
            //{
            //    sqliteContext.Orders.Remove(o1);
            //    sqliteContext.SaveChanges();
            //}

            //var mongoContext = serviceProvider.GetService<Mongo.StoreContext>();
            //var o22 = new Order("ZehDog2", 129);
            //mongoContext.Orders.Add(o22);
            //var o222 = mongoContext.Orders.Find(o22.Id);
            //mongoContext.SaveChanges();

            //var ox2 = mongoContext.Orders.FirstOrDefault(x => x.Id == o22.Id);

            //var o2 = mongoContext.Orders.FirstOrDefault();
            //if (o2 != null)
            //{
            //    mongoContext.Orders.Remove(o2);
            //    var o2222 = mongoContext.Orders.Find(o22.Id);
            //    mongoContext.SaveChanges();
            //}
        }

        private static void SetupSqlite(IServiceCollection services)
        {
            var connection = "Data Source=Store.db";
            services.AddDbContext<Sqlite.StoreContext>(options =>
               options
                    .UseLoggerFactory(MyLoggerFactory)
                    .UseSqlite(connection)
            );
        }

        //public static readonly Microsoft.Extensions.Logging.LoggerFactory MyLoggerFactory =
        //    new LoggerFactory(new[] {
        //new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
        //    });

        //    public static readonly ILoggerFactory MyLoggerFactory
        //= LoggerFactory.Create(builder =>
        //{
        //    builder
        //    //.AddFilter((category, level) =>
        //    //    category == DbLoggerCategory.Database.Command.Name
        //    //    && level == LogLevel.Information)
        //    .AddDebug()
        //    .AddConsole();
        //});

        public static readonly LoggerFactory MyLoggerFactory
            = new LoggerFactory(new[] { new DebugLoggerProvider() });

        private static void SetupMongo(IServiceCollection services)
        {
            var connectionString = "mongodb://localhost:27017";
            var databaseName = "store104";
            services.AddSingleton(new Mongo.MongoSettings(connectionString, databaseName));
            services.AddScoped<Mongo.StoreContext>();

            services.AddScoped<IMongoClient>(provider => new MongoClient(connectionString));
        }
    }
}