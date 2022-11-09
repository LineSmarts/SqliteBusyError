using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EfCoreNotNullNestedOwned
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new ExampleContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                for (int i = 0; i < 5; i++)
                    db.Add(new RootEntity());

                db.SaveChanges();
            }

            // Comment this whole task and application will run without any errors.
            Task.Run(() =>
            {
                using (var db1 = new ExampleContext())
                {
                    db1.RootEntities.First().Child = new ChildEntity();

                    db1.SaveChanges();
                }
            });         

            using (var db2 = new ExampleContext())
            {
                var rootEntities = db2.RootEntities.OrderBy(i => i.Id);

                foreach (var rootEntity in rootEntities)
                {
                    // Wait a bit until db1.SaveChanges() above will finish execution.
                    Thread.Sleep(2000);

                    rootEntity.Child = new ChildEntity();                 

                    // Here will be an error Error: SQLite Error 5: 'database is locked'
                    db2.SaveChanges();
                }
            }
        }
    }

    public class ExampleContext : DbContext
    {
        public DbSet<RootEntity> RootEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source=blogging.db");
    }

    public class RootEntity
    {
        public Int32 Id { get; set; }

        public ChildEntity? Child { get; set; }
    }

    public class ChildEntity
    {
        public Int32 Id { get; set; }

        public String? AProperty { get; set; }
    }
}
