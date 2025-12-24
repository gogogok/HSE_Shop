using OrdersService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Data
{
    public sealed class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasIndex(x => x.OrderId)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(x => x.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}