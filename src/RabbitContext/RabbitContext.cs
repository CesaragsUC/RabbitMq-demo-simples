using Microsoft.EntityFrameworkCore;
using Rabbit.Subscription.Domain;

namespace RabbitAppContext
{
    public class RabbitContext : DbContext
    {
        public RabbitContext(DbContextOptions<RabbitContext> options) : base(options) { }

        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Produto> Produtos { get; set; }

    }
}