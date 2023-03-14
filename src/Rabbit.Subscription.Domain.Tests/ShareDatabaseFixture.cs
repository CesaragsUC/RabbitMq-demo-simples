using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RabbitAppContext;

namespace Rabbit.Subscription.Domain.Tests
{

    public class ShareDatabaseFixture : IDisposable
    {
        private readonly SqliteConnection connection;
        public ShareDatabaseFixture()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
        }

        public void Dispose() => connection.Dispose();

        public RabbitContext CreateContext()
        {
            var result = new RabbitContext(new DbContextOptionsBuilder<RabbitContext>().UseSqlite(connection).Options);
            result.Database.EnsureCreated();
            return result;
                
        }
    }
}