using System.Data.Common;
using ITServicesApp.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Tests.Persistence.Relational
{
    public static class SqliteRelationalDb
    {
        public static (DbConnection connection, DbContextOptions<ApplicationDbContext> options) CreateOpen()
        {
            var conn = new SqliteConnection("DataSource=:memory:;Mode=Memory;Cache=Shared");
            conn.Open();

            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(conn)
                .EnableSensitiveDataLogging()
                .Options;

            return (conn, opts);
        }

        public static void EnsureCreatedWithFks(ApplicationDbContext db)
        {
            db.Database.EnsureCreated();
            db.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        }
    }
}
