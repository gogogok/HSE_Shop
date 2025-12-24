using OrdersService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Data
{
    /// <summary>
    /// Контекст базы данных заказов
    /// </summary>
    public sealed class OrdersDbContext : DbContext
    {
        /// <summary>
        /// Конструктор OrdersDbContext
        /// </summary>
        /// <param name="options">Настройки контекста</param>
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        /// <summary>
        /// Таблица заказов
        /// </summary>
        public DbSet<Order> Orders => Set<Order>();
        
        /// <summary>
        /// Таблица outbox сообщений
        /// </summary>
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        /// <summary>
        /// Конфигурация модели БД
        /// </summary>
        /// <param name="modelBuilder">Строитель модели</param>
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